using UnityEngine;
using TENDOR.Core;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Services.Firebase
{
    /// <summary>
    /// Firebase configuration and initialization (Stub implementation)
    /// TODO: Replace with actual Firebase implementation when Firebase SDK is installed
    /// </summary>
    public class FirebaseConfig : MonoBehaviour
    {
        [Header("Firebase Configuration")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool enablePersistence = true;
        
        // Stub properties - replace with actual Firebase types when SDK is installed
        public static object App { get; private set; }
        public static object Auth { get; private set; }
        public static object Firestore { get; private set; }
        public static object Storage { get; private set; }
        
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
            Logger.Log("🔥 Initializing Firebase (Stub)...", "FIREBASE");
            
            if (enablePersistence)
            {
                Logger.Log("📦 Persistence enabled", "FIREBASE");
            }
            
            // Stub implementation - replace with actual Firebase initialization
            App = new object();
            Auth = new object();
            Firestore = new object();
            Storage = new object();
            
            IsInitialized = true;
            Logger.Log("✅ Firebase stub initialized successfully", "FIREBASE");
            Logger.Log("⚠️  Note: This is a stub implementation. Install Firebase SDK for full functionality.", "FIREBASE");
            
            // Simulate anonymous sign-in
            SignInAnonymously();
        }
        
        private void SignInAnonymously()
        {
            Logger.Log("✅ Anonymous user signed in (stub): user_stub_12345", "FIREBASE");
        }
    }
} 