# SynicSugar
![https://github.com/skeyll/SynicSugar/blob/main/LICENSE](https://img.shields.io/github/license/skeyll/SynicSugar) ![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue) [![openupm](https://img.shields.io/npm/v/net.skeyll.synicsugar?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.skeyll.synicsugar/)  
SynicSugar is the syntax sugar to synchronize a game via the internet. The backend is EOS, so the server cost is free. The goal is an easy online-game dev for everyone!


## Feature
- Max 64 peers full-mesh connect
- No Use Cost and No CCU Limit
- MatchMake with your conditions
- Host-Migration
- Re-connect to a disconnected match
- Cross-platform connction (Android, iOS, Windows, and Console)

```csharp
using SynicSugar.P2P;
using MemoryPack;
using UnityEngine;
[NetworkPlayer]
public partial class Player {    
    [SyncVar(3000)] public Vector3 pos;
    //Sync in manager's interval
    [SyncVar] public int Hp;
    [SyncVar(1000)] Skill skill;
    
    [Rpc] //Can send 1st parameter to other clients
    public void Attack(int a, int b = 0, float c = 0){
         Hp -= a;
    }
    [TargetRpc] //Can send 2nd parameter to id's client
    public void GreetTarget(UserId id){
         Debug.log("Hi");
    }
    [Rpc] //Can pass multiple data class with MemoryPack
    public void Heal(HealInfo info){
         Debug.log($"{OwnerUserId} heal {info.target} {info.amount}");
    }
}    
[MemoryPackable]
public partial class Skill {
    public string Name;
    public bool isValid;
    public int Damage;
}
[MemoryPackable]
public partial class HealInfo {
    public UserId target;
    public int amount;
}
```
```csharp
using SynicSugar.P2P;
using UnityEngine;
[NetworkCommons]
public partial class GameSystem : MonoBehaviour {
    //Sync Host's value by 500ms
    [SyncVar(true, 500)] public float currentTime;
    [SyncVar] Vector3 enemyPos;
    
    [Rpc] 
    public void StartGame(){
         Debug.log("Start");
    }
}
```

## Requirement
- Unity 2021-3 or later
- [UniTask](https://github.com/Cysharp/UniTask)
- [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [eos_plugin_for_unity](https://github.com/PlayEveryWare/eos_plugin_for_unity)
- [Mono.Cecil](https://github.com/jbevain/cecil)

 SynicSugar uses Roslyn SourceGenerator supported after 2021.3. SourceGenerator generates almost all codes for p2p connect on compile automatically.  
 Large dependencies is for performance. SynicSugar is a full-mesh p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with 63 peer in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster.  

## Getting started
### 1.Install SynicSugar and depended librarys.  
 The first is to import SynicSugar and dependent libraries.　You can get SynicSugar from OpenUPM or [SynicSugar/Release](https://github.com/skeyll/SynicSugar/releases)'s unitypackage.  
 .unitypackage contains Mono.Cecil and System.Runtime.CompilerServices.Unsafe.dll for MemoryPack and, in addition to SynicSugar. Therefore, you can skip some processes, but it is more convenient to download via OpenUPM for version control.  

1. Rigister some package with OpenUPM  

 In your unity project, select Edit/ProjectSetting/PackageManager. Then, register some librarys.
 
 Name: OpenUPM
 
 URL: https://package.openupm.com
 
 Scope(s):
* net.skeyll.synicsugar (Skip if downloading as unitypackage)
* com.cysharp.unitask
* com.playeveryware.eos
* com.cysharp.memorypack
           
![image](https://user-images.githubusercontent.com/50002207/230567095-04cfbfcc-f1c9-4b0d-9088-2fbfc08da8f8.png)


2. Install these packages  
　These packages can be imported from **Window/PackageManager/MyRegistries**. Importing SynicSugar will automatically import the other required librarys. If you are using another version in your project, that one will probably work. However, SynicSugar has been developed using the following:  
 * Epic Online Services Plugin for Unity: 2.2.0  
 * UniTask: 2.0.31  
 * MemoryPack: 1.9.13  
 
 
 3. Import the rest (Skip if downloading as unitypackage.)  
Import what is not in OpenUPM.  
- Mono.Cecil  
Enter **com.unity.nuget.mono-cecil** in **Edit/ProjectSetting/PackageManager/+/Add package from git URL**.  

![image](https://user-images.githubusercontent.com/50002207/231324146-292634b7-3d42-420d-a20c-37f5fc0ad688.png)

- System.Runtime.CompilerServices.Unsafe  
MemoryPack need System.Runtime.CompilerServices.Unsafe.dll. You can get this dll from Download package in https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0 . Since this contains DLLs for multiple environments, only import packages for Unity. Unzip the downloaded file and drag and drop **lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll** into your project.


### 2.Get some tokens for EOS.

Please check [the eos document](https://dev.epicgames.com/ja/news/how-to-set-up-epic-online-services-eos) or [the plugin page](https://github.com/PlayEveryWare/eos_plugin_for_unity). SynicSugar doesn't need EOS store brand. Just register and can use server.

About app credential, you can use Peer2Peer as ClientPolicy. The minimum is as follows.
![image](https://user-images.githubusercontent.com/50002207/230758754-4333b431-48fe-4539-aa97-20c6f86d68ae.png)


### 3.Autentication

To authenticate EOS and manage the tick, EOSManager is needed. Therefore, first place the EOSManager.prefab in SynicSugar/Runtime/Prefabs/ to the Scene.

![image](https://user-images.githubusercontent.com/50002207/230759934-0d32e507-7194-4783-8b6c-c666d0685b50.png)

SynicSugar are written in UniTask's Async/Await. If the authentication succeeds, it returns True.
```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    async UniTask LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        bool isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken);

        if(isSuccess){
            //Success
            return;
        }
        //Failure
    }
}
```     
You can also write without Async/Await processing.
```csharp
// using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    public void LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        
        EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken);
        // or Explicitly ignore AsyncAwait. Get more performance in IL.
        // EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken).Forget();
        
        //Continue to process not to wait.
        // ...
    }
 }
```      


### 4. Matchmaking

You need EOSp2pManager for matchmaking and P2P communication. Please add the SynicSugar/Core/Prefabs/EOSp2pManager.prefab to the Matching scene. You can also  create this from script. This object is a singleton and won't be destroyed until explicitly disposed of.

```csharp
using UnityEngine;
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    GameObject matchmakeContainer;
    void Awake(){
        if(MatchMakeManager.Instance == null){
            matchmakeContainer = Instantiate(matchmakePrefab);
        }
    }
}
```
Several settings can be configured in the Unity editor.

![image](https://user-images.githubusercontent.com/50002207/230761023-3754a4fc-46ae-4d33-8f86-9439ce1846c0.png)

To start matchmaking, you can call StartMatching(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null). Firstly, the server's lobby is searched and if there is a Lobby that meets the condition, user join it. If there is no Lobby that meets the condition, user create new lobby. The user waits for TimeoutSec, which is set on EOSp2pManager. If the matching is successful, it returns True, otherwise it returns False.

```csharp
using SynicSugar.MatchMake;

public class MatchMakeConditions : MonoBehaviour {
    public Lobby GetLobbyCondition(){
        //Create conditions
        Lobby lobbyCondition = EOSLobbyExtenstions.GenerateLobby("Rank", "ASIA");
        
        lobbyCondition.MaxLobbyMembers = 2; //2-64
        
        //Add conditions as attributes.
        //The limit of conditions is 100.
        //Need "Key", "Value", and "ComparisonOption"
        LobbyAttribute attribute = new LobbyAttribute();
        attribute.Key = "Level";
        attribute.SetValue(1); //Can use int, string, double, bool
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal; // https://dev.epicgames.com/docs/en-US/game-services/lobbies#comparison-operators
        lobbyCondition.Attributes.Add(attribute);
        
        //attribute = new LobbyAttribute();
        //attribute.Key = "SeaMap";
        //...
        
        return lobbyCondition;
    }
}
```
After creating the mathing conditions, call MatchMakeManager.Instance.StartMatchMake().

```csharp
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    async UniTask StartMatching(){
        CancellationTokenSource matchCancellToken = new CancellationTokenSource();
        
        bool isSuccess = await MatchMakeManager.Instance.StartMatchMake(matchConditions.GetLobbyCondition(), matchCancellToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failure
    }
}
```

### 5. p2p connect

In SynicSugar, there are two types of synchronization: "NetworkPlayer" and "NetworkCommons". NetworkPlayer behaves like NetworkBehavior in other libraries, where each instance has its own user ID and only the user with that ID can change the value. On the other hand, NetworkCommons is shared by all users, and can be used to manage system parts such as game time and enemy HP.

For both types, you need to create a class with the "public partial class" keyword and add "NetworkPlayer" or "NetworkCommons" as an attribute. There are no inheritance requirements.

```csharp
using SynicSugar.P2P;
[NetworkPlayer]
public partial class XXX {
///
}
[NetworkCommons]
public partial class XXX {
///
}
```

If you want to synchronize a structure, you need to serialize it with [MemoryPack](https://github.com/Cysharp/MemoryPack).　See sample for more details.


## Warning
 The Rpc process (like SynicSugarRpcxxx) also are generated now. However, I'll change the such send method to insert of IL weaving to prevent unintentional bugs and cheating. You can call the Rpc process manually, but it may change.

 In addition, SynicSugar is still in α. So, this will be updated with the my developping game.
