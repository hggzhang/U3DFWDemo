local M = _G.Class()
local ObjPool = _G.Core.ObjPool
local UIView = _G.UI.View
local ViewFactory = _G.UI.ViewFactory

function M:Ctor(cs)
    self.cs = cs
    self.bindableList = nil
    self.pool = ObjPool:New()
    self.itemViews = {}
end

function M:OnValChanged(bindableList)
    self.bindableList = bindableList
    self:UpdateView()
end

function M:FindPool()
    local item = self.pool:Find()
    if item == nil then
        item = UIView:New()
    end
    return item
end

function M:FreePool(item)
    self.pool:Free(item)
end

function M:UpdateView()
    if self.bindableList == nil then return end
    for _, view in pairs(self.itemViews) do
        ViewFactory.Free(view)
    end
    table.clear(self.itemViews)
    local len = self.bindableList:Len()
    if len == 0 then
        return
    end
    self.cs:UpdateView(len)
    local views = self.cs:GetChildViews()
    local luaFile = views[1]:GetLuaFile()
    for idx = 1, len do
        local view = ViewFactory.Get(luaFile)
        view:BindCSView(views[idx - 1])
        view:SetParams({Adapter = self, Data = self.bindableList[idx]})
        view:Init()
        view:Begin()
        table.insert(self.itemViews, view)
    end
    -- table.clear(self.itemViews)
end

return M