using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using TENDOR.Core;

namespace TENDOR.Services.Firebase
{
    /// <summary>
    /// Firebase configuration and initialization
    /// </summary>
    public class FirebaseConfig : MonoBehaviour
    {
        [Header("Firebase Configuration")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool enablePersistence = true;
        
        public static FirebaseApp App { get; private set; }
        public static FirebaseAuth Auth { get; private set; }
        public static FirebaseFirestore Firestore { get; private set; }
        public static FirebaseStorage Storage { get; private set; }
        
        public static bool IsInitialized { get; private set; }
        
        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeFirebase();
            }
        }
        
        public void InitializeFirebase()
        {
            Logger.Log("🔥 Initializing Firebase...", "FIREBASE");
            
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp
                    App = FirebaseApp.DefaultInstance;
                    
                    // Initialize Firebase services
                    Auth = FirebaseAuth.DefaultInstance;
                    Firestore = FirebaseFirestore.DefaultInstance;
                    Storage = FirebaseStorage.DefaultInstance;
                    
                    // Enable offline persistence
                    if (enablePersistence)
                    {
                        Firestore.Settings = new FirestoreSettings
                        {
                            PersistenceEnabled = true
                        };
                    }
                    
                    IsInitialized = true;
                    Logger.Log("✅ Firebase initialized successfully", "FIREBASE");
                    
                    // Sign in anonymously for testing
                    SignInAnonymously();
                }
                else
                {
                    Logger.LogError($"❌ Could not resolve Firebase dependencies: {dependencyStatus}", "FIREBASE");
                }
            });
        }
        
        private void SignInAnonymously()
        {
            Auth.SignInAnonymouslyAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Logger.LogError("❌ Firebase anonymous sign-in was canceled", "FIREBASE");
                    return;
                }
                if (task.IsFaulted)
                {
                    Logger.LogError($"❌ Firebase anonymous sign-in failed: {task.Exception}", "FIREBASE");
                    return;
                }

                FirebaseUser newUser = task.Result;
                Logger.Log($"✅ Anonymous user signed in: {newUser.UserId}", "FIREBASE");
            });
        }
    }
} 