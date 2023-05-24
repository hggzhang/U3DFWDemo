

local BindableProperty = _G.Class()

function BindableProperty:Ctor(val)
    self.binders = {}
    self.val = val
    self.oldVal = nil
end

function BindableProperty:Modify(val)
    self.oldVal = self.val
    self.val = val
    self:OnValChanged()
end

function BindableProperty:Bind(binder)
    table.insert(self.binders, binder)
    binder:OnValChanged(self.val)
end

function BindableProperty:UnBind(binder)
    table.remove(self.binders, binder)
end

function BindableProperty:OnValChanged()
    for _, binder in pairs(self.binders) do
        binder:OnValChanged(self.val, self.oldVal)
    end
end

return BindableProperty