
require "msg"

local service_id

function OnInit(id)
    service_id = id
    engine.Listen(8888, id)
end

function OnExit()
end

local login_id = 2
function OnSocketData(fd, buff)
    print("gate way OnSocketData")
    local msg = process_buff(buff)
    local isLogin = false
    if msg then
        if not isLogin then
            send_service_msg(login_id, msg, fd)
        end
    else
        print("len not enough")
    end
end

function send_service_msg(target_id, msg, client_fd)
    local pack = {
        gate = service_id,
        client_fd = client_fd,
        msg = msg,
    }

    local packMsg = m_json.encode(pack)
    engine.Send(1, 2, packMsg)
end

function OnAcceptMsg(listenFd, clentFd)
    print("gateway Accept")
end

function OnServiceMsg(source, msg)
    print(" OnServiceMsg gate msg = " .. tostring(msg))
    local clent_fd, resp_msg = unpack_msg_service(msg)
    print("resp_msg = " .. tostring(resp_msg))
    OnServiceMsgTest(resp_msg)
    engine.Write(service_id, clent_fd, resp_msg)
end

function OnServiceMsgTest(msg)
    print("debug packet")

    local inf = process_buff(msg)
    local name, body = process_msg(inf)
end
