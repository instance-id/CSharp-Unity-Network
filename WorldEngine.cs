using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Barebones.Logging;
using Barebones.MasterServer;
using cfx;
using Framework.Server.Core.RPC;
using Framework.Server.Core;
using Framework.Server.Core.Data;
using Framework.Server.Core.ServerToServer;
using Framework.Server.Core.ServerToServer.Operations;
using Framework.Server.Game;
using Framework.Server.Networking.Interfaces;
using log4net;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework.Server
{
    [MessagePackObject]
    public class WorldEngine : cfxNotificationMPTransponder
    {

        private ISession session { get; set; }
        private WorldSession worldSession { get; set; }
        private PeerBase peerBase { get; set; }
        private int outgoingMessageCount;
        private RoomController controller;
        private NOC _noc { get; set; }
        private ApplicationBase application;
        //private readonly INetworkControl server;
        
        private string ApplicationName { get; set; }
        public UnetGameRoom gameroom;
        private static readonly object padlock = new object();
        //private static WorldEngine _instance;
        //public static WorldEngine Instance => _instance;

        public override void Start()
        {
            base.Start();
            Debug.Log("Testing New setup");
            //ApplicationName = "WorldEngine";
            SetupLogging();
            //server.Start("Master");
            
            //_noc = new NOC();

            //masterIncPeer = new IncomingSubServerPeer(masterApplication);
            //Debug.LogWarning("Master Incoming Peer Created");
            //worldApp = _worldApplication.CreateOutgoingMasterPeer(_worldApplication);
            //_worldServerPeer = WorldServerPeer.Instance;

//            #region Serialization Resolvers
//            CompositeResolver.RegisterAndSetAsDefault(
//            PrimitiveObjectResolver.Instance,
//            BuiltinResolver.Instance,
//            AttributeFormatterResolver.Instance,
//            UnityResolver.Instance,
//            StandardResolver.Instance
//            );
//            #endregion

//            //TODO Log Message
//            RegisterHandlers();
//            Debug.Log("Registering handler complete");

            //Debug.Log(_masterApplication.ApplicationName);
//            
//            Debug.Log("Getting Gameroom");
//            Debug.Log("Loading World");
//            //BeginLoad();
//            Debug.Log("Complete");
        }

//        private void BeginLoad()
//        {
//            _masterApplication = new MasterApplication();
//            var loginRequest = InitRequest("Login");
//            _loginApplication = new LoginApplication(loginRequest);
//            _worldApplication = new WorldApplication();
//        }
//
//        private InitResponse InitRequest(string app)
//        {
//           var initRequest = new InitRequest(app, new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue), Protocol.Chunked, (ushort) 0, false);
//            Logger.Debug("InitRequest Made");
//            var initResponse = new InitResponse(initRequest.ApplicationId, initRequest.Protocol, initRequest.InitObject);
//            return initResponse;
//        }

        #region Event Handlers

        public void SendOperationResponse(OperationResponse operationResponse, SendParameters parameters) => _noc.OperationResponse(operationResponse, parameters);
        
        //public void SendMasterOperationRequest(OperationRequest operationRequest, SendParameters parameters) => masterIncPeer.OnOperationRequest(operationRequest, parameters);
        //public void SendWorldOperationRequest(OperationRequest operationRequest, SendParameters parameters) => worldSession.OnOperationRequest(operationRequest, parameters);

        
        public void SendEventData(EventData eventData, SendParameters parameters) => _noc.EventOperation(eventData, parameters);

        public void SendTestNotification(int conn)
        {
            // ---  Initial test! -------------------------------------
            var sessionID = conn;
            var opCode = (byte) RtsMessageType.OperationResponse;
            var opresponse = (byte)ResultCode.Ok;
            var ChannelID = PeerSettings.MmoObjectEventChannel;
            var debug = "Something goofed";
            var parameters = new Dictionary<byte, object>
            {
                {0, opCode},
                {1, opresponse},
                {2, debug},
            };

            var operationResponse = new OperationResponse((byte) GameOperationCode.EnterWorld, parameters);
            this.SendOperationResponse(operationResponse, new SendParameters
            {
                ChannelId = ChannelID,
                SessionId = sessionID
            });
        }
        //------------------------------------------------------------------------------
        protected ClientLoginData LoginData
        {
            get
            {
                return this.loginData;
            }
            private set
            {
                Interlocked.Exchange(ref this.loginData, value);
            }
        }
        private ClientLoginData loginData;
        
        public void SendTestEvent(int conn)
        {
            // ---  Initial test -------------------------------------
            var sessionID = conn;
            var opCode = (byte) RtsMessageType.OperationResponse;
            var opresponse = (byte)ResultCode.Ok;
            var ChannelID = PeerSettings.MmoObjectEventChannel;
            var debug = "Something goofed";

            this.LoginData = new ClientLoginData
            {
                ClientId = 151,
                Username = "Bob",
                CharacterName = "instance.id"
            };
            
            SendEvent(
                new EventData((byte)ClientEventCode.CharacterLoggedIn,
                    new Dictionary<byte, object>
                    {
                        {(byte) ParameterCode.CharacterName, this.LoginData.CharacterName}
                    }),
                new SendParameters
                {
                    SessionId = sessionID
                });
        }
        
        public void CreateSessionEvent(int conn)
        {
            // ---  Initial test! -------------------------------------
            var sessionID = conn;
            var opCode = (byte) RtsMessageType.OperationResponse;
            var opresponse = (byte)ResultCode.Ok;
            var ChannelID = PeerSettings.MmoObjectEventChannel;
            var debug = "Something goofed";

            this.LoginData = new ClientLoginData
            {
                ClientId = 151,
                Username = "Bob",
                CharacterName = "instance.id"
            };
            
            SendEvent(
                new EventData((byte)ClientEventCode.CharacterLoggedIn,
                    new Dictionary<byte, object>
                    {
                        {(byte) ParameterCode.CharacterName, this.LoginData.CharacterName},
                        //{(byte) ParameterCode.ClientId, this.LoginData.CharacterName},
                    }),
                new SendParameters
                {
                    SessionId = sessionID
                });
        }
        
        #endregion

        #region Message Handlers


        private void RegisterHandlers()
        {
            NetworkServer.RegisterHandler((byte)RtsMessageType.Operation, IncomingOperationHandler);
        }

        private void IncomingOperationHandler(NetworkMessage msg)
        {
            var peer = msg.conn.connectionId;
            var byteArray = msg.ReadMessage<ByteMessage>();
            var recievedBytes = byteArray.Data;
            //var deserial = _noc.DeserializeOperation(recievedBytes);
//            //TODO Log Message
//            Debug.Log(LZ4MessagePackSerializer.ToJson(deserial));
//            	var operationData = new RegisterServer
//            		{
//            			ServerId = application.ServerId,
//            			SubServerId = application.SubServerId,
//            			ServerType = (byte) application.ServerType,
//            			Address = serverInfo.PublicIP,
//            			TcpPort = serverInfo.TcpPort,
//            			UdpPort = serverInfo.UdpPort,
//            		};
//
//            	var request = new OperationRequest((byte) ServerOperationCode.RegisterServer, operationData);
//            	NOC.Instance.SendOperationRequest(request, new SendParameters());
//            SendTestNotification(peer);
//            SendTestEvent(peer);
        }   
        
        private void IncomingSessionHandler(NetworkMessage msg)
        {
            var peer = msg.conn.connectionId;
            var byteArray = msg.ReadMessage<ByteMessage>();
            var recievedBytes = byteArray.Data;
            var deserial = _noc.DeserializeSession(recievedBytes);
            //TODO Log Message
            Debug.Log(LZ4MessagePackSerializer.ToJson(deserial));

            
//            SendOperationResponse(new OperationRequest((byte) ServerOperationCode.AckClientUserLogin,
//                    new AckClientUserLogin
//                    {
//                        SessionId = peer,
//                        Username = userData.Username
//                    }),
//                new SendParameters());

            //---------------------- Session Response --------------------
            var sessionID = peer;
            var opCode = (byte) RtsMessageType.OperationResponse;
            var opresponse = (byte)ResultCode.Ok;
            var ChannelID = PeerSettings.MmoObjectEventChannel;
            var debug = "Something goofed";

            this.LoginData = new ClientLoginData
            {
                ClientId = 151,
                Username = "Bob",
                CharacterName = "instance.id"
            };
            
            SendEvent(
                new EventData((byte)ClientEventCode.CharacterLoggedIn,
                    new Dictionary<byte, object>
                    {
                        {(byte) ParameterCode.CharacterName, this.LoginData.CharacterName}
                    }),
                new SendParameters
                {
                    SessionId = sessionID
                });
            
            //---------------------- Add Session ---- --------------------
            var param = deserial.parameters;
            var charName = param[22];
            var condition = param[59];
            var Parameters = new Dictionary<byte, object>
            {
                {(byte)ParameterCode.SessionId, peer}, 
                {(byte)ParameterCode.CharacterName, charName},
                {(byte)ParameterCode.Condition, condition},
                //{(byte)0, masterPeer.ClientId},
            };
//            _worldServerPeer.OnServerEvent(
//                new EventData(deserial.sessActionTemp, Parameters),
//                new SendParameters
//                {
//                    SessionId = peer,
//                    //ClientId = masterPeer.ClientId
//                });

            Debug.Log("Sent");
        }   

        #endregion

        public void SendOpResponse(OperationResponse operatResponse, SendParameters parameters)
        {
            this.SendOperationResponse(operatResponse, parameters);
        }

        public void SendEvent(EventData eventData, SendParameters parameters)
        {
            this.SendEventData(eventData, parameters);
        }

        //public override void OnNotification(string notificationName)
        //{
        //    if (notificationName == "SentToServer")
        //    {
        //        DebugConsole.Log("Received Notification! Sending one back!");
        //        netSendNotification("SentFromServer");
        //    }

        //    if (notificationName =="ServerOpRequest")
        //    {
        //        Debug.Log("Message received!");
        //        Debug.Log("Sending reply");
        //        sendNotification("ServerOpRequestReply");
        //        Debug.Log("Reply sent");
        //    }

        //    if (notificationName == UniqueNotificationName("ServerEventResponse"))
        //    {
        //    }
        //}

        #region Logging setup

        private void SetupLogging()
        {
            Debug.Log("Initializing Logging");
            //var file = new FileInfo(Path.Combine(this.BinaryPath, "config"));
            //var file = new FileInfo(Path.Combine("C:\\Users\\Home\\Documents\\Unity\\A_RPG\\RPG_Game_Server\\Assets\\_Components\\_Server\\_Server\\Config\\log4net.config"));
            var file = new FileInfo("test");
            
            if (file.Exists)
            {
                Debug.Log("Logging Initialized");

                Core.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                GlobalContext.Properties["LogFileName"] = "MS_" + this.ApplicationName;
                log4net.Config.XmlConfigurator.ConfigureAndWatch(file);
            }
        }
        
        #endregion
    }
}
