local NetMgr = {}
local LuaCallCS = _G.CS.LuaCallCS
local cjson = require("cjson")

function NetMgr:Init()
    self.data = {}
end

function NetMgr:Reg(k, cb)
    
    if self.data[k] == nil then
        self.data[k] = {}
    end

    local events = self.data[k]
    table.insert(events, cb)
    
    return function()
        table.remove_item(events, cb)
    end
end

function NetMgr:SendMsg(msg, msgBody)
    if msg and msgBody then
        local Param = {
            ["msg"] = msg,
            ["msgBody"] = msgBody,
        }
        LuaCallCS.Call("SendNetMsg", Param)
    end
end

function NetMgr:OnRecv(code)
    local msg = cjson.decode(code)
    local name = msg.name
    

    
    if self.data[name] then
        for _, cb in pairs(self.data[name]) do
            cb(msg)
        end
    end
end

NetMgr:Init()

return NetMgr