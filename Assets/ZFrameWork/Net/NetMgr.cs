using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;

public class RWBuffer
{
    const int DEFAULT_SIZE = 1024;
    const int MOVE_BYTE_TEST = 8;

    public int initSize;
    public byte[] bytes;
    public int rIdx;
    public int wIdx;
    public int capacity;
    public int remain { get { return capacity - wIdx; } }
    public int len { get { return wIdx - rIdx; } }

    public RWBuffer(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        initSize = size;
        capacity = size;
        rIdx = 0;
        wIdx = 0;
    }

    public RWBuffer(byte[] inBytes)
    {
        bytes = inBytes;
        initSize = inBytes.Length;
        capacity = initSize;
        rIdx = 0;
        wIdx = initSize;
    }

    public void Resize(int inSize)
    {
        if (inSize < len) return;
        if (inSize < initSize) return;

        int test = 1;
        while (test < inSize) test *= 2;
        capacity = test;
        byte[] temp = new byte[test];
        Array.Copy(bytes, rIdx, temp, 0, wIdx - rIdx);
        bytes = temp;
        wIdx = len;
        rIdx = 0;
    }

    public int Write(byte[] inBytes, int off, int cnt)
    {
        if (remain < cnt) Resize(len + cnt);
        Array.Copy(inBytes, off, bytes, wIdx, cnt);
        wIdx += cnt;
        return cnt;
    }

    public void CheckThenMoveBytes()
    {
        if (len < MOVE_BYTE_TEST)
            MoveBytes();
    }

    public void MoveBytes()
    {
        Array.Copy(bytes, rIdx, bytes, 0, len);
        wIdx = len;
        rIdx = 0;
    }

    public int Read(byte[] inBytes, int off, int cnt)
    {
        cnt = Math.Min(cnt, len);
        Array.Copy(inBytes, 0, bytes, off, cnt);
        rIdx += cnt;
        CheckThenMoveBytes();
        return cnt;
    }

    public Int16 ReadInt16(byte[] inBytes, int off, int cnt)
    {
        if (len < 2) return 0;
        Int16 ret = BitConverter.ToInt16(bytes, rIdx);
        rIdx += 2;
        CheckThenMoveBytes();
        return ret;
    }

    public Int32 ReadInt32(byte[] inBytes, int off, int cnt)
    {
        if (len < 4) return 0;
        Int32 ret = BitConverter.ToInt32(bytes, rIdx);
        rIdx += 4;
        CheckThenMoveBytes();
        return ret;
    }

    public override string ToString()
    {
        return BitConverter.ToString(bytes, rIdx, len);
    }

    
}

public class MsgBase
{
    public string name;
    public static byte[] Encode(MsgBase msg)
    {
        string s = JsonUtility.ToJson(msg);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    public static MsgBase Decode(string proto, byte[] bytes, int offset, int cnt)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, cnt);
        MsgBase msg = (MsgBase)JsonUtility.FromJson(s, Type.GetType(proto));
        return msg;
    }

    // 协议名 2字节长度 + 字符串
    public static byte[] EncodeName(MsgBase msg)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.name);
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[2 + len];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    public static string DecodeName(byte[] bytes, int off, out int cnt)
    {
        cnt = 0;
        if (off + 2 > bytes.Length) return "";
        Int16 len = (Int16)((bytes[off + 1] << 8) | bytes[off]);
        if (off + 2 + len > bytes.Length) return "";
        cnt = cnt + 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, off + 2, len);
        return name;
    }
}

public class MsgPing : MsgBase
{
    public MsgPing() { name = "MsgPing"; }
}


public class MsgPong : MsgBase
{
    public MsgPong() { name = "MsgPong"; }
}

public class MsgLogin : MsgBase
{
    public MsgLogin() { name = "MsgLogin"; }

    public string user = "";
    public string password = "";
}

public class MsgLoginNtf : MsgBase
{
    public MsgLoginNtf() { name = "MsgLoginNtf"; }
    public int code  = 0;
    public string playerName = "";
    public int gold = 0;
}

public enum ENetEve
{
    ConnSucc = 1,
    ConnFail = 2,
    Close = 3,
}

public class NetMgr : MgrBase<NetMgr>
{
    const int MAX_MSG_SEND = 10;
    Socket socket;
    RWBuffer recvBuf;
    Queue<RWBuffer> sendQue;
    bool isConnecting = false;
    bool isClosing = false;
    List<MsgBase> msgs;
    int msgCnt = 0;

    bool isUsePing = true;
    int pingInv = 30;
    float lastPing = 0;
    float lastPong = 0;

    public delegate void NetEveCB(string err);
    Dictionary<ENetEve, NetEveCB> netEveCBDict;

    public delegate void NetMsgCB(MsgBase Msg);
    private Dictionary<string, NetMsgCB> netMsgCBDict;

    protected override void OnInit()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        recvBuf = new RWBuffer();
        sendQue = new Queue<RWBuffer>();
        isConnecting = false;
        isClosing = false;
        msgs = new List<MsgBase>();
        msgCnt = 0;

        isUsePing = true;
        pingInv = 30;
        lastPing = 0;
        lastPong = 0;

        netEveCBDict = new Dictionary<ENetEve, NetEveCB>();
        netMsgCBDict = new Dictionary<string, NetMsgCB>();
    }

    protected override void OnBegin()
    {
        Connect("127.0.0.1", 8888);
    }

    public void AddNetEveCB(ENetEve et, NetEveCB cb)
    {
        if (netEveCBDict.ContainsKey(et))
        {
            netEveCBDict[et] += cb;
        }
        else
        {
            netEveCBDict[et] = cb;
        }
    }

    public void RemoveNetEveCB(ENetEve et, NetEveCB cb)
    {
        if (netEveCBDict.ContainsKey(et))
        {
            netEveCBDict[et] -= cb;
        }
    }

    void OnNetEve(ENetEve et, String err)
    {
        if (netEveCBDict.ContainsKey(et))
        {
            netEveCBDict[et](err);
        }
    }

    public void AddNetMsgCB(string name, NetMsgCB cb)
    {
        if (netMsgCBDict.ContainsKey(name))
        {
            netMsgCBDict[name] += cb;
        }
        else
        {
            netMsgCBDict[name] = cb;
        }
    }

    public void RemoveNetMsgCB(string name, NetMsgCB cb)
    {
        if (netMsgCBDict.ContainsKey(name))
        {
            netMsgCBDict[name] -= cb;
        }
    }

    void OnNetMsg(string name, MsgBase Msg)
    {
        if (netMsgCBDict.ContainsKey(name))
        {
            netMsgCBDict[name](Msg);
        }
        LuaEvent.NetMsg(Msg);
    }

    void OnRecv(IAsyncResult ar)
    {
        try
        {
            Socket inSocket = (Socket)ar.AsyncState;
            int cnt = inSocket.EndReceive(ar);
            recvBuf.wIdx += cnt;
            OnRecvData();
            if (recvBuf.remain < 8)
            {
                recvBuf.MoveBytes();
                recvBuf.Resize(recvBuf.len * 2);
            }
            inSocket.BeginReceive(recvBuf.bytes, recvBuf.wIdx, recvBuf.remain, 0, OnRecv, inSocket);
        }
        catch (Exception E)
        {
            ZLogger.Err("Socket Receice Fail : " + E.ToString());
        }
    }

    void OnRecvData()
    {
        if (recvBuf.len <= 2)
            return;
        int rIdx = recvBuf.rIdx;
        byte[] inBytes = recvBuf.bytes;
        Int16 bodyLen = (Int16)((inBytes[rIdx + 1] << 8) | inBytes[rIdx]); //大小端
        if (recvBuf.len < bodyLen)
            return;
        int nameCnt = 0;
        recvBuf.rIdx += 2;
        string protoName = MsgBase.DecodeName(recvBuf.bytes, recvBuf.rIdx, out nameCnt);
        if (protoName == "")
        {
            ZLogger.Err("OnReceiveData MsgBase.DecodeName Fail");
            return;
        }

        recvBuf.rIdx += nameCnt;
        int bodyCount = bodyLen - nameCnt - 2;
        MsgBase Msg = MsgBase.Decode(protoName, recvBuf.bytes, recvBuf.rIdx, bodyCount);

        recvBuf.rIdx += bodyCount;
        recvBuf.CheckThenMoveBytes();
        lock (msgs)
        {
            msgs.Add(Msg);
            msgCnt++;
        }
        if (recvBuf.len > 2)
            OnRecvData();
    }

    public void Connect(string IP, int Port)
    {
        if (socket != null && socket.Connected)
        {
            ZLogger.Err("Connect Fail, Already Connected!");
            return;
        }
        if (isConnecting)
        {
            ZLogger.Err("Connect Fail, IsConnecting!");
            return;
        }

        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(IP, Port, OnConnect, socket);
    }

    void OnConnect(IAsyncResult ar)
    {
        try
        {
            Socket inSocket = (Socket)ar.AsyncState;
            inSocket.EndConnect(ar);
            ZLogger.Log("Socket Connect Succ!");
            OnNetEve(ENetEve.ConnSucc, "");
            isConnecting = false;
            inSocket.BeginReceive(recvBuf.bytes, recvBuf.wIdx, recvBuf.remain, 0, OnRecv, inSocket);
        }
        catch (Exception E)
        {
            ZLogger.Err("Socket Connect Fail: " + E.ToString());
            OnNetEve(ENetEve.ConnFail, "");
            isConnecting = false;
        }
    }

    public void Close()
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        if (sendQue.Count > 0)
            isClosing = true;
        else
        {
            socket.Close();
            OnNetEve(ENetEve.Close, "");
        }
    }

    void OnSend(IAsyncResult ar)
    {
        Socket Socket = (Socket)ar.AsyncState;
        if (Socket == null || !Socket.Connected || sendQue.Count == 0)
            return;

        int cnt = Socket.EndSend(ar);
        RWBuffer buf;
        lock (sendQue)
            buf = sendQue.First();

        buf.rIdx += cnt;
        if (buf.len == 0)
            lock (sendQue)
            {
                sendQue.Dequeue();
                if (sendQue.Count > 0)
                    buf = sendQue.First();
                else
                    buf = null;
            }

        if (buf != null)
            Socket.BeginSend(buf.bytes, buf.rIdx, buf.len, 0, OnSend, Socket);
        else if (isClosing)
            Socket.Close();
    }

    public void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected) return;
        if (isConnecting) return;
        if (isClosing) return;


        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int Len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + Len];

        //组装长度
        sendBytes[0] = (byte)(Len % 256);
        sendBytes[1] = (byte)(Len / 256);

        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        RWBuffer buf = new RWBuffer(sendBytes);
        int cnt = 0;
        lock (sendQue)
        {
            sendQue.Enqueue(buf);
            cnt = sendQue.Count;
        }
        if (cnt == 1)
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, OnSend, socket);
    }

    public void MsgUpdate()
    {
        if (msgCnt == 0)
            return;

        for (int i = 0; i < MAX_MSG_SEND; i++)
        {
            MsgBase Msg = null;
            lock (msgs)
                if (msgs.Count > 0)
                {
                    Msg = msgs[0];
                    msgs.RemoveAt(0);
                    msgCnt--;
                }

            if (Msg != null)
                OnNetMsg(Msg.name, Msg);
            else
                break;
        }
    }

    void PingUpdate()
    {
        /*if (!isUsePing)
        {
            return;
        }

        int Time = TimeUtil.Second();
        if (Time - LastPingTime > PingInv)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPing = Time;
        }

        if (Time - lastPong > pingInv * 4)
        {
            Close();
        }*/
    }

    public void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

}
