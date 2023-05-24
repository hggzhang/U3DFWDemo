local M = _G.Class()
local EventMgr = _G.EventMgr
function M:Ctor()
    self.events = {}
end

function M:Reg(k, cb)
    if self.events[k] then
        return
    end
    local h = EventMgr:Reg(k, cb)
    self.events[k] = h
end

function M:UnReg(k)
    if self.events[k] then
        self.events[k]();
        self.events[k] = nil
    end
end

function M:UnRegAll()
    for _, h in ipairs(self.events) do
        h()
    end
    table.clear(self.events)
end

return M