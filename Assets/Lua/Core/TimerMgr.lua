local M = _G.Class(_G.MgrBase)
local ObjPool = _G.Core.ObjPool
function M:Ctor()
    self.data = {}
    self.pool = ObjPool
end

function M:Reg(cb, delay, inv, loop)
    local now = 1
    local timerInfo = {
        stamp = now + inv + delay,
        inv = inv,
        cb = cb,
        loop = loop,
    }

    self.data[timerInfo] = timerInfo
    return function()
        self.data[timerInfo] = nil
    end
end

function M:Tick(delta)
    local now = 1
    
    for _, info in pairs(self.data) do
        if now >= info.stamp then
            info.cb()
            if info.loop then
                if info.loop > 0 then
                    info.stamp = now + info.inv
                    info.loop = info.loop - 1
                else
                    self.data[info] = nil
                end
            end
        end
    end
end

return M