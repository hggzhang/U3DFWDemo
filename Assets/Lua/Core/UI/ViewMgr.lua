
local ViewMgr = _G.Class()
local CSViewMgr = _G.CS.ViewMgr
local Log = _G.Log
local UIUtil = _G.UIUtil
local ViewFactory = _G.UI.ViewFactory
function ViewMgr:Ctor()
    self.viewDict = {}
end

-- TODO:MgrBase..
ViewMgr:Ctor()

function ViewMgr:LoadView(id, cb, params)
    local cfg = _G.UI.ViewConfig[id]
    if cfg == nil then
        Log.Error("LoadView cfg nil")
        return
    end

    CSViewMgr:LoadView(cfg.res, function(csView)
        local luaFile = csView:GetLuaFile()
        local view = ViewFactory.Get(luaFile)
        view:BindCSView(csView)
        self.viewDict[id] = view
        cb(self, view, params)
    end)
end

function ViewMgr:OnCompleteShow(view, params)
    view:Show(params)
end

function ViewMgr:Show(id, params)
    if self.viewDict[id] ~= nil then
        self.viewDict[id]:Show(params)
    end

    self:LoadView(id, self.OnCompleteShow, params)
end

function ViewMgr:Hide(id)
    if self.viewDict[id] == nil then
        return
    end

    self.viewDict[id]:Hide()
end

return ViewMgr