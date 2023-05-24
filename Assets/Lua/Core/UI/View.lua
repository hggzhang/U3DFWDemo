
local View = _G.Class()
local ViewFactory = _G.UI.ViewFactory
local BinderRegister = _G.Core.BinderRegister
local EventRegister = _G.Core.EventRegister


function View:Ctor(csView)
    self.childern = {}
    if csView ~= nil then
        self:BindCSView(csView)
    end
    self.binderRegister = BinderRegister:New()
    self.eventRegisiter = EventRegister:New()
end

function View:Init()
    self:OnInit()
end

function View:BindCSView(csView)
    self.csView = csView
    self.csView:BindLuaView(self)
end

function View:UnBindCSView()
    for _, child in pairs(self.childern) do
        child:UnBindCSView()
    end
    table.clear(self.childern)
    self.csView:UnBindLuaView(self)
    self.csView = nil
end

function View:Begin()
    self:OnBegin()
end

function View:End()
    self:OnEnd()
end

function View:Shutdown()
    if self.csView ~= nil then
        self.csView:UnBindLuaView()
        self.csView = nil
    end
    self:OnShutdown()
end

function View:OnInit()
    -- body
end

function View:OnBegin()
    -- body
end

function View:OnEnd()
    -- body
end

function View:OnShutdown()
    -- body
end

function View:OnRegBinders()
    -- body
end

function View:RegBinder(vm, key, binder)
    self.binderRegister:RegBinder(vm, key, binder)
end

function View:UnRegBinder(vm, key, binder)
    self.binderRegister:UnRegBinder(vm, key, binder)
end

function View:RegBinders(vm, binders)
    self.binderRegister:RegBinders(vm, binders)
end

function View:UnRegBinders(vm, binders)
    self.binderRegister:UnRegBinders(vm, binders)
end

function View:UnRegAllBinders()
    self.binderRegister:UnRegAllBinders()
end

function View:OnRegTimer()
    -- body
end

function View:RegTimer()
    -- body
end

function View:UnRegAllTimer()
    -- body
end

function View:UnRegTimer()
    -- body
end

function View:OnRegEvent()
    -- body
end

function View:RegEvent(k, cb)
    self.eventRegisiter:Reg(k, cb)
end

function View:UnRegAllEvent()
    self.eventRegisiter:UnRegAll()
end

function View:UnRegEvent(k)
    self.eventRegisiter:UnReg(k)
end

function View:AddSubView()
    --
end

function View:RemoveSubView()
    --
end

function View:Show(params)
    self:SetParams(params)
    self:OnRegEvent()
    self:OnRegBinders()
    self:OnRegTimer()
    self.csView:Show()
    for _, child in pairs(self.childern) do
        child:Show()
    end
    self:OnShow()
end

function View:OnShow()
    --
end

function View:Hide()
    self:UnRegAllTimer()
    self:UnRegAllBinders()
    self:UnRegAllEvent()
    self.csView:Hide()
    for _, child in pairs(self.childern) do
        child:Hide()
    end
    self:OnHide()
end

function View:OnHide()
    
end

function View:AddChildView(csView, luaFile)
    local viewClass = ViewFactory.Get(luaFile)
    local view = viewClass:New(csView)
    table.insert(self.childern, view)
end

function View:RemoveAllChildView()
    for _, child in pairs(self.childern) do
        ViewFactory.Free(child)
    end
    self.childern = {}
end

function View:SetParams(params)
    self.params = params
end

-- [[UIOperation]] --
function View:SetImage(widget, res)
    self.csView:SetImage(widget, res)
end

function View:SetText(widget, text)
    self.csView:SetText(widget, text)
end

function View:SetVisible(widget, v)
    self.csView:SetVisible(widget, v)
end

function View:AddClickEvent(widget, cb)
    self.csView:AddClick(widget, cb)
end

function View:GetText(widget)
    return self.csView:GetText(widget)
end

return View