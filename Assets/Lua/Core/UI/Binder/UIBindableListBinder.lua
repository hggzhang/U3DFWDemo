local UIBindableListBinder = _G.Class(_G.Core.UI.Binder)

function UIBindableListBinder:Ctor(widget)
    -- body
end

function UIBindableListBinder:OnValChanged(new, old)
    self.widget:OnValChanged(new)
end

return UIBindableListBinder