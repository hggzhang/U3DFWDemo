local M = {}

function M:Ctor(caster, target, skill, expressionStr)
    self.caster = caster
    self.target = target
    self.skill = skill

    self.func = self:MakeFunc(expressionStr)
end

local VarIdent = {
    ["ATK"] = "Attack",
    ["DEF"] = "Defence",
}

local ObjIdent = {
    ["m"] = "self.caster",
    ["t"] = "self.target",
    ["s"] = "self.skill"
}

function M:MakeFunc(expressionStr)
    local code = string.gsub(expressionStr, "(%a*)%.(%a*)", function(obj, var)
        return ObjIdent[obj] .. ":" .. VarIdent[var]
    end)

    local cat = {}
    table.insert(cat, "return function(self)\n")
    table.insert(cat, "    return ")
    table.insert(cat, code)
    table.insert(cat, "\nend")
    code = table.concat(cat)
    local func = load(code)()
    return func
end

return