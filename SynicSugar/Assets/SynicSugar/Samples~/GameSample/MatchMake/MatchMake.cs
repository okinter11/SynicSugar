using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace  SynicSugar.Samples {
    public class MatchMake : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        GameObject matchmakeContainer;
        [SerializeField] Button startMatchMake, closeLobby, startGame, backtoMenu;
        [SerializeField] MatchGUIState descriptions;
        [SerializeField] MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText, matchmakeState;
        [SerializeField] GameModeSelect modeSelect; //For tankmatchmaking scene, and to cancel matchmake then return to menu.
        //For Tank
        public InputField nameField;
        [SerializeField] Text playerName;
        enum SceneState{
            Standby, inMatchMake, ToGame
        }
    #region Init
        void Awake(){
            if(MatchMakeManager.Instance == null){
                matchmakeContainer = Instantiate(matchmakePrefab);
            }
            SetGUIState();
            MatchMakeManager.Instance.LobbyMemberUpdateNotifier.Register(t => OnUpdatedMemberAttribute(t));
        }
        void SetGUIState(){
            descriptions = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            descriptions.state = matchmakeState;
            descriptions.stopAdditionalInput.AddListener(StopAdditionalInput);
            descriptions.acceptCancel.AddListener(() => ActivateCancelButton(false));

            MatchMakeManager.Instance.SetGUIState(descriptions);
        }
    #endregion
    #region For Recconect
        async void Start(){
            //Dose this game allow user to re-join thedisconnected match? 
            if(MatchMakeManager.Instance.lobbyIdSaveType == MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection){
                return;
            }

            //Try recconect
            //Sample projects use the playerprefs to save LobbyIDs for recconection.
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
            //On the default way, return Empty when there is no lobby data in local.
            if(string.IsNullOrEmpty(LobbyID)){
                return;
            }

            EOSDebug.Instance.Log($"SavedLobbyID is {LobbyID}.");
            
            startMatchMake.gameObject.SetActive(false);
            CancellationTokenSource token = new CancellationTokenSource();

            bool canReconnect = await MatchMakeManager.Instance.ReconnectLobby(LobbyID, token);

            if(canReconnect){
                EOSDebug.Instance.Log($"Success Recconect! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
                SwitchGUIState(SceneState.ToGame);
                return;
            }
            EOSDebug.Instance.Log("Fail to Re-Connect Lobby.");
            SwitchGUIState(SceneState.Standby);
        }
    #endregion
        public void StartMatchMake(){
            EOSDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();

            //For some samples to need user name before MatchMaking.
            //Just set user name for game.
            if(nameField != null){
                nameField.gameObject.SetActive(false);
                TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
                playerName.text = $"PlayerName: {nameField.text}";
            }
        }
        //We can't set NOT void process to Unity Event.
        //So, register StartMatchMake() to Button instead of this.
        //Or, change this to async void StartMatchMakeEntity() at the expense of performance. We can pass async void to UnityEvents.
        async UniTask StartMatchMakeEntity(){
            //We have two ways to call SearchAndCreateLobby.
            //If pass self caneltoken, we should use Try-catch.
            //If not, the API returns just bool result.
            //Basically, we don't have to give token to SynicSugar APIs.
            bool selfTryCatch = false;

            if(!selfTryCatch){ //Recommend
                bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), userAttributes: GenerateUserAttribute());
                
                if(!isSuccess){
                    EOSDebug.Instance.Log("MatchMaking Failed.");
                    SwitchGUIState(SceneState.Standby);
                    return;
                }
            }else{ //Sample for another way
                try{
                    CancellationTokenSource matchCTS = new CancellationTokenSource();
                    bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), matchCTS, GenerateUserAttribute());

                    if(!isSuccess){
                        EOSDebug.Instance.Log("Backend may have something problem.");
                        SwitchGUIState(SceneState.Standby);
                        return;
                    }
                }catch(OperationCanceledException){
                    EOSDebug.Instance.Log("Cancel MatchMaking");
                    SwitchGUIState(SceneState.Standby);
                    return;
                }
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);

            GoGameScene();
        }
        /// <summary>
        /// Cancel matchmaking (Host delete and Guest leave the current lobby)
        /// </summary>
        public void CancelMatchMaking(){
            CancelMatchMakingEntity().Forget();

            if(nameField != null){
                nameField.gameObject.SetActive(true);
            }
        }
        async UniTask CancelMatchMakingEntity(){
            SwitchCancelButtonActive(false);

            bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake();
            
            SwitchGUIState(SceneState.Standby);
        }
        /// <summary>
        /// Cancel matchmaking (Host delete and Guest leave the current lobby)
        /// Then destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public async void CanelMatchMakingAndReturnToLobby(){
            SwitchCancelButtonActive(false);

            bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }
        //State event
        public void ActivateCancelButton(bool afterMatching){
            SwitchCancelButtonActive(true);
            buttonText.text = afterMatching ? "Close Lobby" : "Stop MatchMake";
        }
        public void StopAdditionalInput(){
            startMatchMake.gameObject.SetActive(false);
        }
        
        void SwitchCancelButtonActive(bool isActivate){
            //To return main menu
            if(backtoMenu != null){
                backtoMenu.gameObject.SetActive(isActivate);
            }
            closeLobby.gameObject.SetActive(isActivate);
        }
        
        void GoGameScene(){
            if(startGame != null){ //For ReadHeart and Chat
                startGame.gameObject.SetActive(true);
            }else{ //For Tank
                modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
            }
        }
        List<AttributeData> GenerateUserAttribute(){
            //We can set max 100 attributes.
            List<AttributeData> attributeData = new();
            //Name
            AttributeData attribute = new (){
                Key = "NAME"
            };
            string Name = GetRandomString();
            attribute.SetValue(Name);
            attributeData.Add(attribute);
            //Rank
            attribute = new (){
                Key = "RANK"
            };
            int Rank = UnityEngine.Random.Range(0, 10);
            attribute.SetValue(Rank);
            attributeData.Add(attribute);
            //Win count
            attribute = new (){
                Key = "WIN"
            };
            int Win = UnityEngine.Random.Range(0, 100);
            attribute.SetValue(Win);
            attributeData.Add(attribute);
            
            EOSDebug.Instance.Log($"UserName: {Name.ToString()} / Rank: {Rank} / Win: {Win}");

            return attributeData;
            
            string GetRandomString(){
                var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string name = System.String.Empty;
                var random = new System.Random();

                for (int i = 0; i < 6; i++){
                    name += sample[random.Next(sample.Length)];
                }
                return name;
            }
        }
        void OnUpdatedMemberAttribute(UserId target){
            List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
            string name = AttributeData.GetValueAsString(data, "NAME");

            foreach(var attr in data){
                EOSDebug.Instance.Log($"{name}: {attr.Key} {attr.GetValueAsString()}");
            }
        }
        void SwitchGUIState(SceneState state){
            if(startMatchMake != null){
                startMatchMake.gameObject.SetActive(state == SceneState.Standby);
            }
            if(closeLobby != null){
                closeLobby.gameObject.SetActive(state == SceneState.ToGame);
            }
            if(startGame != null){
                startGame.gameObject.SetActive(state == SceneState.ToGame);
            }
            if(backtoMenu != null){
                backtoMenu.gameObject.SetActive(false);
            }
        }
    }
}
