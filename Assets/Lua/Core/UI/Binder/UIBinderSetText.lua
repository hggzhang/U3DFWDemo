local UIBinderSetText = _G.Class(_G.UI.Binder.UIBinder)

function UIBinderSetText:Ctor(view, widget)
    -- body
end

function UIBinderSetText:OnValChanged(new, old)
    self.view:SetText(self.widget, tostring(new))
end

return UIBinderSetText