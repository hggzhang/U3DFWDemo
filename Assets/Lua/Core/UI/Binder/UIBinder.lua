
local Binder = _G.Core.Binder
local UIBinder = _G.Class(Binder)

function UIBinder:Ctor(view, widget)
    self.view = view
    self.widget = widget
end

return UIBinder