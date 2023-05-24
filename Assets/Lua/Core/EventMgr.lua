local EventMgr = {}

function EventMgr:Init()
    self.data = {}
end

function EventMgr:Reg(k, cb)
    if self.data[k] == nil then
        self.data[k] = {}
    end

    local events = self.data[k]
    table.insert(events, cb)

    return function()
        table.remove_item(events, cb)
    end
end

function EventMgr:Send(k, ...)
    if self.data[k] == nil then
        return
    end

    -- 暂时不考虑在事件里订阅事件
    for _, cb in pairs(self.data[k]) do
        cb(...)
    end
end

EventMgr:Init()

return EventMgr