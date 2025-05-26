using UnityEngine;
using System;
using TENDOR.Core;
using TENDOR.Runtime.Models;
using TENDOR.Services.Firebase;
using TENDOR.Services.AR;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Services
{
    /// <summary>
    /// Single-scene state machine managing the core application flow
    /// States: Idle -> Recording -> Processing -> Playback -> Idle
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("State Configuration")]
        [SerializeField] private GameState initialState = GameState.Idle;
        [SerializeField] private bool enableStateLogging = true;

        // Current state
        private GameState currentState = GameState.Idle;
        private GameState previousState = GameState.Idle;

        // State data
        private ClimbData currentClimb;
        private string currentVideoPath;
        private bool isTransitioning = false;

        // Events
        public event Action<GameState, GameState> OnStateChanged;
        public event Action<string> OnRecordingStarted;
        public event Action<string> OnRecordingCompleted;
        public event Action<ClimbData> OnProcessingStarted;
        public event Action<ClimbData> OnProcessingCompleted;
        public event Action<ClimbData> OnPlaybackStarted;

        // State handlers
        private IGameState[] stateHandlers;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStateHandlers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ChangeState(initialState);
        }

        private void InitializeStateHandlers()
        {
            stateHandlers = new IGameState[4];
            stateHandlers[(int)GameState.Idle] = new IdleState();
            stateHandlers[(int)GameState.Recording] = new RecordingState();
            stateHandlers[(int)GameState.Processing] = new ProcessingState();
            stateHandlers[(int)GameState.Playback] = new PlaybackState();

            Logger.Log("Game state handlers initialized", "STATE");
        }

        #region State Management

        /// <summary>
        /// Change to a new state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (isTransitioning)
            {
                Logger.LogWarning($"State transition already in progress, ignoring request to change to {newState}", "STATE");
                return;
            }

            if (currentState == newState)
            {
                Logger.LogWarning($"Already in state {newState}", "STATE");
                return;
            }

            isTransitioning = true;

            try
            {
                // Exit current state
                if (stateHandlers[(int)currentState] != null)
                {
                    stateHandlers[(int)currentState].OnExit();
                }

                previousState = currentState;
                currentState = newState;

                if (enableStateLogging)
                {
                    Logger.Log($"State changed: {previousState} -> {currentState}", "STATE");
                }

                // Enter new state
                if (stateHandlers[(int)currentState] != null)
                {
                    stateHandlers[(int)currentState].OnEnter();
                }

                OnStateChanged?.Invoke(previousState, currentState);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error during state transition: {e.Message}", "STATE");
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>
        /// Get current state
        /// </summary>
        public GameState GetCurrentState() => currentState;

        /// <summary>
        /// Check if in specific state
        /// </summary>
        public bool IsInState(GameState state) => currentState == state;

        #endregion

        #region State Transitions

        /// <summary>
        /// Start recording (Idle -> Recording)
        /// </summary>
        public void StartRecording()
        {
            if (currentState != GameState.Idle)
            {
                Logger.LogWarning($"Cannot start recording from state {currentState}", "STATE");
                return;
            }

            // Create new climb data
            currentClimb = new ClimbData
            {
                id = Guid.NewGuid().ToString(),
                ownerUid = FirebaseService.Instance.GetCurrentUserId(),
                status = ClimbStatus.Uploading
            };

            ChangeState(GameState.Recording);
            OnRecordingStarted?.Invoke(currentClimb.id);
        }

        /// <summary>
        /// Stop recording and start processing (Recording -> Processing)
        /// </summary>
        public void StopRecording(string videoPath)
        {
            if (currentState != GameState.Recording)
            {
                Logger.LogWarning($"Cannot stop recording from state {currentState}", "STATE");
                return;
            }

            currentVideoPath = videoPath;
            OnRecordingCompleted?.Invoke(currentClimb.id);
            
            ChangeState(GameState.Processing);
            OnProcessingStarted?.Invoke(currentClimb);
        }

        /// <summary>
        /// Complete processing and start playback (Processing -> Playback)
        /// </summary>
        public void CompleteProcessing(string fbxUrl, string jsonUrl)
        {
            if (currentState != GameState.Processing)
            {
                Logger.LogWarning($"Cannot complete processing from state {currentState}", "STATE");
                return;
            }

            if (currentClimb != null)
            {
                currentClimb.fbxUrl = fbxUrl;
                currentClimb.jsonUrl = jsonUrl;
                currentClimb.status = ClimbStatus.Ready;
            }

            OnProcessingCompleted?.Invoke(currentClimb);
            ChangeState(GameState.Playback);
            OnPlaybackStarted?.Invoke(currentClimb);
        }

        /// <summary>
        /// Return to idle (Any state -> Idle)
        /// </summary>
        public void ReturnToIdle()
        {
            // Clean up current climb data
            currentClimb = null;
            currentVideoPath = null;

            ChangeState(GameState.Idle);
        }

        /// <summary>
        /// Handle processing error (Processing -> Idle)
        /// </summary>
        public void HandleProcessingError(string errorMessage)
        {
            if (currentClimb != null)
            {
                currentClimb.status = ClimbStatus.Error;
                FirebaseService.Instance.UpdateClimbStatusAsync(currentClimb.id, ClimbStatus.Error);
            }

            Logger.LogError($"Processing failed: {errorMessage}", "STATE");
            ReturnToIdle();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get current climb data
        /// </summary>
        public ClimbData GetCurrentClimb() => currentClimb;

        /// <summary>
        /// Get current video path
        /// </summary>
        public string GetCurrentVideoPath() => currentVideoPath;

        /// <summary>
        /// Check if state transitions are allowed
        /// </summary>
        public bool CanTransition() => !isTransitioning;

        #endregion

        private void Update()
        {
            // Update current state
            if (stateHandlers[(int)currentState] != null)
            {
                stateHandlers[(int)currentState].OnUpdate();
            }
        }

        private void OnDestroy()
        {
            // Clean up state handlers
            if (stateHandlers != null)
            {
                foreach (var handler in stateHandlers)
                {
                    handler?.OnExit();
                }
            }
        }
    }

    /// <summary>
    /// Game states for the single-scene state machine
    /// </summary>
    public enum GameState
    {
        Idle = 0,       // Show menu (Record / Library / Latest Play)
        Recording = 1,  // Recording video + hip tracking
        Processing = 2, // Upload + DeepMotion processing
        Playback = 3    // Playing back recorded climb
    }

    /// <summary>
    /// Interface for state handlers
    /// </summary>
    public interface IGameState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    #region State Implementations

    public class IdleState : IGameState
    {
        public void OnEnter()
        {
            Logger.Log("Entered Idle state - showing main menu", "STATE");
            // TODO: Show main menu UI
            // TODO: Enable menu interactions
        }

        public void OnUpdate()
        {
            // Handle idle state updates
        }

        public void OnExit()
        {
            Logger.Log("Exiting Idle state", "STATE");
            // TODO: Hide main menu UI
        }
    }

    public class RecordingState : IGameState
    {
        public void OnEnter()
        {
            Logger.Log("Entered Recording state - starting video and hip tracking", "STATE");
            // TODO: Ensure runtime image library is loaded
            // TODO: Start video recording
            // TODO: Start hip tracking
            // TODO: Show recording UI
        }

        public void OnUpdate()
        {
            // Handle recording updates
            // TODO: Check for image tracking
            // TODO: Update recording UI
        }

        public void OnExit()
        {
            Logger.Log("Exiting Recording state", "STATE");
            // TODO: Stop video recording
            // TODO: Stop hip tracking
            // TODO: Hide recording UI
        }
    }

    public class ProcessingState : IGameState
    {
        public void OnEnter()
        {
            Logger.Log("Entered Processing state - uploading and processing", "STATE");
            // TODO: Show processing UI with progress
            // TODO: Start upload process
            // TODO: Poll DeepMotion API
        }

        public void OnUpdate()
        {
            // Handle processing updates
            // TODO: Update progress UI
            // TODO: Check processing status
        }

        public void OnExit()
        {
            Logger.Log("Exiting Processing state", "STATE");
            // TODO: Hide processing UI
        }
    }

    public class PlaybackState : IGameState
    {
        public void OnEnter()
        {
            Logger.Log("Entered Playback state - preparing playback", "STATE");
            // TODO: Download/stream FBX + JSON
            // TODO: Spawn avatar
            // TODO: Align to locked pose
            // TODO: Show playback controls
        }

        public void OnUpdate()
        {
            // Handle playback updates
            // TODO: Update playback controls
            // TODO: Handle play/pause/scrub
        }

        public void OnExit()
        {
            Logger.Log("Exiting Playback state", "STATE");
            // TODO: Stop playback
            // TODO: Hide playback UI
            // TODO: Clean up avatar
        }
    }

    #endregion
} 