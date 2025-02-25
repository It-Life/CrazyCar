﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using QFramework;

public enum ServerType {
    Local = 0,
    Remote
}

public enum NetType {
    WebSocket = 0,
    KCP
}

public enum MsgType{
    CreatePlayer = 0,
    PlayerState = 1,
    DelPlayer = 2,
    MatchRoomCreate = 3,
    MatchRoomJoin = 4,
    MatchRoomStatus = 5,
    MatchRoomExit = 6,
    MatchRoomStart = 7,
    // 用户操作
    PlayerOperat = 8,
    PlayerCompleteGame = 9
}


public class NetworkController : MonoBehaviour, IController {
    public ServerType serverType;
    public NetType netType;

    private void Awake() {
        this.GetSystem<INetworkSystem>().ServerType = serverType;
        this.GetSystem<INetworkSystem>().NetType = netType;
        this.GetSystem<INetworkSystem>().HttpBaseUrl = Util.GetServerBaseUrl(serverType);
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        // KCP 开了线程所以只能把 RespondAction放进主线程
        if (this.GetSystem<INetworkSystem>().PlayerCreateMsgs.Count > 0) {
            lock (this.GetSystem<INetworkSystem>().MsgLock) {
                this.SendCommand(new MakeNewPlayerCommand(this.GetSystem<INetworkSystem>().PlayerCreateMsgs.Peek()));
                this.GetSystem<INetworkSystem>().PlayerCreateMsgs.Dequeue();
            }
        }

        if (this.GetSystem<INetworkSystem>().PlayerStateMsgs.Count > 0) {
            lock (this.GetSystem<INetworkSystem>().MsgLock) {
                this.GetSystem<IPlayerManagerSystem>().RespondStateAction(
                    this.GetSystem<INetworkSystem>().PlayerStateMsgs.Peek());
                this.GetSystem<INetworkSystem>().PlayerStateMsgs.Dequeue();
            }
        }

        if (this.GetSystem<INetworkSystem>().PlayerOperatMsgs.Count > 0) {
            lock (this.GetSystem<INetworkSystem>().MsgLock) {
                this.GetSystem<IPlayerManagerSystem>().RespondOperatAction(
                    this.GetSystem<INetworkSystem>().PlayerOperatMsgs.Peek());
                this.GetSystem<INetworkSystem>().PlayerOperatMsgs.Dequeue();
            }
        }

        if (this.GetSystem<INetworkSystem>().PlayerCompleteMsgs.Count > 0) {
            lock (this.GetSystem<INetworkSystem>().MsgLock) {
                this.SendCommand(new UpdateMatchResultUICommand(this.GetSystem<INetworkSystem>().PlayerCompleteMsgs.Peek()));
                this.GetSystem<INetworkSystem>().PlayerCompleteMsgs.Dequeue();
            }
        }
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}
