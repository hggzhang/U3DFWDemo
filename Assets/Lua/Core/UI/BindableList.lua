local M = _G.Class()

function M:Ctor(vmClass)
    self.vmClass = vmClass
    self.items = {}
    self.pool = {}
    self.cbs = {}
end

function M:FindPool()
    if #self.pool == 0 then
        return self.vmClass:New()
    end

    return table.remove(self.pool)
end

function M:FreePool(item)
    table.insert(self.pool, item)
end

function M:UpdateAll(vals)
    local delta = #vals - #self.items
    if delta ~= 0 then
        if delta > 0 then
            for _ = 1, delta do
                table.insert(self.items, self:FindPool())
            end
        else
            delta = -delta
            for _ = 1, delta do
                local item = table.remove(self.items)
                self:FreePool(item)
            end
        end
    end

    for idx = 1, #vals do
        self.items[idx]:UpdateVM(vals[idx])
    end

    for _, cb in pairs(self.cbs) do
        cb(self)
    end
end

function M:Len()
    return #self.items
end

function M:Get(idx)
    return self.items[idx]
end

function M:GetItems()
    return self.items
end

function M:RegValChangedCB(cb)
    table.insert(self.cbs, cb)
end

function M:UnRegValChangedCB(cb)
    table.insert(self.cbs, cb)
end

return M