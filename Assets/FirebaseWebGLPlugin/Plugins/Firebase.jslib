


mergeInto(LibraryManager.library, {
    Initialize: function (config, callbackObject, successCallbackMethod, failCallbackMethod) {
        try {
            globalThis.firebaseApp = firebase.initializeApp({
    apiKey: "AIzaSyBPc_vlcMMGUJ3PqJXiba7_qq8YywgXRg0",
    authDomain: "subgrids-9eb6d.firebaseapp.com",
    databaseURL: "https://subgrids-9eb6d-default-rtdb.firebaseio.com",
    projectId: "subgrids-9eb6d",
    storageBucket: "subgrids-9eb6d.firebasestorage.app",
    messagingSenderId: "516419573854",
    appId: "1:516419573854:web:3a585818d6885de934fccf",
    measurementId: "G-EBVLMCVCQ6"
  });

            globalThis.firebaseAuth = firebase.auth();
            globalThis.firebaseDB = firebase.database();

            console.log("Firebase Initialized");
            SendMessage(UTF8ToString(callbackObject), "FirebaseInitialized", "SUCCESS");
        } catch (error) {
            console.error("Firebase Initialization Error:", error.message);
            SendMessage(UTF8ToString(callbackObject), "FirebaseInitializationFailed", "ERROR: " + error.message);
        }
    },

    SignUp: function (email, password, callbackObject, callbackMethod) {
        console.log("Sign up function being called");
        firebaseAuth.createUserWithEmailAndPassword(UTF8ToString(email), UTF8ToString(password))
            .then((userCredential) => {
                let user = userCredential.user;
                user.sendEmailVerification()
                    .then(() => {
                        
                        SendMessage("FirebaseManager", "SignUpComplete", user.uid);
                    });

                
            })
            .catch((error) => {
                console.error("Signup Error:", error.message);
                SendMessage("FirebaseManager", "SignUpFailed", "ERROR: " + error.message);
            });
    },

    SignIn: function (email, password, callbackObject, callbackMethod) {

        console.log(`${email} ${password}`);
        console.log(`${UTF8ToString(email)} ${UTF8ToString(password)}`);

        firebaseAuth.signInWithEmailAndPassword(UTF8ToString(email), UTF8ToString(password))
            .then((userCredential) => {
                let user = userCredential.user;
                if (user.emailVerified) {
                    
                    const converted = UTF8ToString(user.uid);
                console.log(`UID ${user.uid} - ${converted}`);

                    SendMessage("FirebaseManager", "SignInComplete", ( user.uid));
                } else {
                    console.warn("User email not verified. Signing out.");
                    firebaseAuth.signOut();
                   SendMessage("FirebaseManager", "SignInFailed", "EMAIL_NOT_VERIFIED");
                }
            })
            .catch((error) => {
                console.error("Signin Error:", error.message);
                SendMessage("FirebaseManager", "SignInFailed",  "ERROR: " + error.message);
            });
    },

    WriteData: function (userId, jsonString, callbackObject, callbackMethod) {

        console.log(`Current Unique ID ${userId}`);
                console.log(UTF8ToString(jsonString));

        firebaseDB.ref("users/" + UTF8ToString(userId)).set(JSON.parse(UTF8ToString(jsonString)))
            .then(() => {
                console.log("Data successfully written!");
                SendMessage("FirebaseManager", "DataWriteComplete", "SUCCESS");
                console.log(UTF8ToString(jsonString));
            })
            .catch((error) => {
                console.error("Error writing data:", error.message);
                SendMessage("FirebaseManager", "DataWriteFailed", "ERROR: " + error.message);
            });
    }
    ,

    ReadData: function (userId, callbackObject, callbackMethod) {
        console.log(`Current Unique ID ${userId} - ${UTF8ToString(userId)}`);

        firebaseDB.ref("users/" + UTF8ToString(userId)).get()
            .then((snapshot) => {
                if (snapshot.exists()) {
                    console.log("User Data:", JSON.stringify(snapshot.val()));
                    SendMessage("FirebaseManager", "DataReadComplete", JSON.stringify(snapshot.val()));
                } else {
                    console.log("No user data found");
                    SendMessage("FirebaseManager", "DataReadFailed", "NO_DATA");
                }
            })
            .catch((error) => {
                console.error("Error fetching data:", error.message);
                SendMessage("FirebaseManager", "DataReadFailed", "ERROR: " + error.message);
            });
    }
});
