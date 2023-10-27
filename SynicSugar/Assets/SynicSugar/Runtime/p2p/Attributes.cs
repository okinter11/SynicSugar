using System;
namespace SynicSugar.P2P {
    //For base
    [AttributeUsage(AttributeTargets.Class,
    Inherited = false)]
    /// <summary>
    /// For each user data like game-character and user-data.
    /// </summary>
    public sealed class NetworkPlayerAttribute : Attribute {
        public bool useGetInstance;
        public NetworkPlayerAttribute(){
        }
        public NetworkPlayerAttribute(bool useGetInstance){
            this.useGetInstance = useGetInstance;
        }
    }
    [AttributeUsage(AttributeTargets.Class,
    Inherited = false)]
    /// <summary>
    /// For shared data like game state and time.
    /// </summary>
    public sealed class NetworkCommonsAttribute : Attribute {
        public bool useGetInstance;
        public NetworkCommonsAttribute(){
        }
        public NetworkCommonsAttribute(bool useGetInstance){
            this.useGetInstance = useGetInstance;
        }
    }
    //For sync
    [AttributeUsage(AttributeTargets.Field,
    Inherited = false)]
    public sealed class SyncVarAttribute : Attribute {
        //Only NetworkCommons
        public bool isHost;
        //If not set, the Manager's value is used. 
        public int syncInterval;
        public SyncVarAttribute(){
        }
        /// <summary>
        /// For NetworkCommons 
        /// </summary>
        /// <param name="isOnlyHost"></param>
        public SyncVarAttribute(bool isOnlyHost){
            isHost = isOnlyHost;
        }
        public SyncVarAttribute(bool isOnlyHost, int syncIntervalMs){
            isHost = isOnlyHost;
            syncInterval = syncIntervalMs;
        }
        public SyncVarAttribute(int syncIntervalMs){
            syncInterval = syncIntervalMs;
        }
    }
    [AttributeUsage(AttributeTargets.Field,
    Inherited = false)]
    public sealed class SynicAttribute : Attribute {
        byte SyncedHierarchy;
        /// <summary>
        /// Synchronize synic variables all at once to TargetUser . This can send packets larger than 1170. </ br>
        /// This method is to pass basic data to reconnecter. </ br>
        /// *Only for public field. See the SynicSugar document for other conditions.</ br> 
        /// NOTE: The performance of this is very bad. 
        /// Since currently the serializer is external library, the classes for serialization cannot be generated by the source generator. </ br>
        /// So, now SynicSugar convert Synic variables to Json, compress the class that has the Jsons then serialize it by MemoryPack.
        /// </summary>
        /// <param name="syncedHierarchy">Hierarchy or step to which this variable belongs. Currently, can use only 0-9.</ br>
        /// If call SyncSynic with 5, all SynicVariable from 0 to 5 are synchronized.</param>
        public SynicAttribute(byte syncedHierarchy = 0){
            SyncedHierarchy = syncedHierarchy;
        }
    }
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class RpcAttribute : Attribute {
        public bool isLargePacket;
        public bool recordLastPacket;
        /// <summary>
        /// For NetworkPlayer and NetoworkCommons. Just Send packet. 
        /// </summary>
        public RpcAttribute(){}
        /// <summary>
        /// Set options for sending packets to resend and the way to send.
        /// </summary>
        /// <param name="shouldRecordLastPacketInfo">If true, the packet info is hold for manual resend.</param>
        /// <param name="isLargePacket">If true, this process is sent as LargePacket.</param>
        public RpcAttribute(bool isLargePacket, bool shouldRecordLastPacketInfo){
            this.isLargePacket = isLargePacket;
            recordLastPacket = shouldRecordLastPacketInfo;
        }
    }
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class TargetRpcAttribute : Attribute {
        public bool isLargePacket;
        public bool recordLastPacket;
        /// <summary>
        /// For NetworkPlayer.
        /// </summary>
        public TargetRpcAttribute(){}
        
        /// <summary>
        /// Set options for sending packets to resend and the way to send.
        /// </summary>
        /// <param name="isLargePacket">If true, this process is sent as LargePacket.</param>
        /// <param name="shouldRecordLastPacketInfo">If true, the packet info is hold for manual resend.</param>
        public TargetRpcAttribute(bool isLargePacket, bool shouldRecordLastPacketInfo){
            this.isLargePacket = isLargePacket;
            recordLastPacket = shouldRecordLastPacketInfo;
        }
    }
    
    /// <summary>
    /// SourceGenerator attach this automatically. If necessary, pass True to NetworkPlayerAttributes.
    /// </summary>
    public interface IGetPlayer{
    }
    /// <summary>
    /// SourceGenerator attach this automatically. If necessary, pass True to NetworkCommonsAttributes.
    /// </summary>
    public interface IGetCommons{
    }
}