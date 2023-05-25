
require "msg"

local service_id

function OnInit(id)
    service_id = id
end

function OnExit()
end

function OnServiceMsg(source, msg)
    print(" OnServiceMsg login " .. tostring(msg))
    resp_msg_service(service_id, msg)
end

resp["MsgLogin"] = function(msgBody)
    print("player login id = " .. tostring(msgBody.user))
    local user = msgBody.user
    local passworld = msgBody.password

    local code = 0
    if user == "admin" and passworld == "admin" then
        code = 1
    end
    return "MsgLoginNtf", {code = code, playerName = "jack", gold = 999}
end
