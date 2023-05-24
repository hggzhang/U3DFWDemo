local UIBinderSetImage = _G.Class(_G.UI.Binder.UIBinder)

function UIBinderSetImage:Ctor(view, widget)
    -- body
end

function UIBinderSetImage:OnValChanged(new, old)
    self.view:SetImage(self.widget, new)
end

return UIBinderSetImage