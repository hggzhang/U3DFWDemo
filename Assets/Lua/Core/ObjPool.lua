local M = _G.Class()

function M:Ctor()
    self.objs = {}
end

function M:Find()
    if #self.objs > 0 then
        return table.remove(self.objs)
    end
end

function M:Free(obj)
    table.insert(self.objs, obj)
end

return M