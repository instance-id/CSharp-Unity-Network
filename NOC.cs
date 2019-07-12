using System;
using System.Collections.Generic;
using Framework.Server.Core.RPC;
using Framework.Server.Network;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework.Server.Core
{
    [MessagePackObject]
    public class NOC : INOC
    {
        private WorldEngine _engine;

        public NOC()
        {
            CompositeResolver.RegisterAndSetAsDefault(
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance,
                AttributeFormatterResolver.Instance,
                UnityResolver.Instance,
                DynamicEnumAsStringResolver.Instance,
                DynamicGenericResolver.Instance,
                DynamicUnionResolver.Instance,
                DynamicObjectResolver.Instance,
                DynamicContractlessObjectResolver.Instance
            );
        }

        public static NOC Instance { get; } = new NOC();

        #region Serlization Process

        public void SendOperationRequest(OperationRequest operationRequest, SendParameters parameterstemp)
        {
            SerializeOperationRequest(operationRequest);
        }
        
        public void OperationResponse(OperationResponse operationResponse, SendParameters parameters)
        {
            SerializeOperationResponse(operationResponse, parameters);
        }

        public void EventOperation(EventData eventData, SendParameters parameterstemp)
        {
            SerializeEvent(eventData, parameterstemp);
        }
        
        #endregion

        #region Begin Serialization
        
        public byte[] SerializeInternal(Core.OperationRequest operationRequest)
        {
            var msg = new OperationRequestMsg {opReq = operationRequest };
            var serializedmsg = LZ4MessagePackSerializer.Serialize(msg);
            return serializedmsg;
        }
    
        public byte[] SerializeOperationRequest(OperationRequest operationRequest)
        {
            //TODO Log Message
            var msg = new OperationRequestMsg{ opReq = operationRequest };
            var serializedmsg = LZ4MessagePackSerializer.Serialize(msg);
            return serializedmsg;
        }

        public byte[] SerializeOperationResponse(OperationResponse operationResponse, SendParameters parameterstemp)
        {
            var sessionid = parameterstemp.SessionId;
            //TODO Log Message
            Debug.Log("Serializing Message");
            var msg = new OperationResponseMsg{ opResp = operationResponse };
            var serializedmsg = LZ4MessagePackSerializer.Serialize(msg);
            Debug.LogFormat("Serialized Message: {0}", LZ4MessagePackSerializer.ToJson(serializedmsg));
            return serializedmsg;
        }

        public void SerializeEvent(EventData eventData, SendParameters parameterstemp)
        {
            var sessionid = parameterstemp.SessionId;
            //TODO Log Message
            Debug.Log(sessionid);
            var msg = new EventRequestMsg { eventData =  eventData };
            var serializedmsg = LZ4MessagePackSerializer.Serialize(msg);
            SendEvent(sessionid, serializedmsg, parameterstemp.ChannelId);
        }

        #endregion

        #region Begin Deserialization
        
        public Temporary DeserializeTemp(byte[] deserial)
        {
            var msg = LZ4MessagePackSerializer.Deserialize<Temporary>(deserial);
            //TODO Log Message
            Debug.Log("Message Deserialized");
            return msg;
        }

        public OperationRequest DeserializeOperationRequest(byte[] deserial)
        {
            Debug.Log(LZ4MessagePackSerializer.ToJson(deserial));
            var msg = LZ4MessagePackSerializer.Deserialize<OperationRequestMsg>(deserial);
            //TODO Log Message
            var test = msg.opReq;
            Debug.Log(test);
            return test;
        }
        
        public OperationResponse DeserializeOperationResponse(byte[] deserial)
        {
            var msg = LZ4MessagePackSerializer.Deserialize<OperationResponse>(deserial);
            //TODO Log Message
            Debug.Log("Message Deserialized");
            return msg;
        }
        
        public SessionMsg DeserializeSession(byte[] deserial)
        {
            var msg = LZ4MessagePackSerializer.Deserialize<SessionMsg>(deserial);
            //TODO Log Message
            Debug.Log("Message Deserialized");
            return msg;
        }

        #endregion

        #region Send Operation Response and Events

        public void SendOpResponse(int conn, byte[] serializedmsg, int channel)
        {
            //TODO Log Message
            Debug.Log(LZ4MessagePackSerializer.ToJson(serializedmsg));
            var data = new ByteMessage() { Data = serializedmsg };
            NetworkServer.SendToClient(conn, (byte)RtsMessageType.OperationResponse, data);
            //TODO Log Message
            Debug.Log("Message Sent");
        }

        public void SendEvent(int conn, byte[] serializedmsg, int channel)
        {
            //TODO Log Message
            Debug.Log(LZ4MessagePackSerializer.ToJson(serializedmsg));
            var data = new ByteMessage() { Data = serializedmsg };
            NetworkServer.SendToClient(conn, (byte)RtsMessageType.Event, data);
        }

        #endregion

        #region Message Classes
        
        [MessagePackObject]
        public class SessionMsg
        {
            [Key(0)] public byte msgTypeTemp { get; set; }

            [Key(1)] public byte sessActionTemp { get; set; }

            [Key(2)] public Dictionary<byte, object> parameters { get; set; }
        }

        [MessagePackObject]
        public class OperationRequestMsg : MessageBase
        {
            [Key(0)]
            public OperationRequest opReq { get; set; }
        }
        
        [MessagePackObject]
        public class OperationResponseMsg : MessageBase
        {
            [Key(0)]
            public OperationResponse opResp { get; set; }
        }

        [MessagePackObject]
        public class EventRequestMsg : MessageBase
        {
            [Key(0)]
            public EventData eventData { get; set; }
        }
        
        [MessagePackObject]
        public class Temporary
        {
            [Key(0)]
            public TemporaryServerPeer tempPeer { get; set; }
        }
        
        #endregion
    }
}
