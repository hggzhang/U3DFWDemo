
function table.remove_item(t, item)
    for k, v in pairs(t) do
        if v == item then
            table.remove(t, k)
            return
        end
    end
end

function table.clear(t)
    for k, _ in pairs(t) do
        t[k] = nil
    end
end

function table.shallow_copy(t)
    local ret = {}
    for k, v in pairs(t) do
        ret[k] = v
    end

    return ret
end

local function doSpace(InConcat, Level)
    local Space = "  "

    for _ = 1, Level do
        table.insert(InConcat, Space)
    end
end

local function table2StrInner(Table, Level, InConcat, MaxLevel)
    table.insert(InConcat, "\n")
    doSpace(InConcat, Level - 1)
    table.insert(InConcat,"{\n")
    for K, V in pairs(Table) do
        doSpace(InConcat, Level)
        if type(K) == "number" then
            table.insert(InConcat, "[")
            table.insert(InConcat, tostring(K))
            table.insert(InConcat, "]")
            table.insert(InConcat, " = ")
        else 
            table.insert(InConcat, tostring(K))
            table.insert(InConcat, " = ")
        end

        if type(V) == "number" or type(V) == "boolean" then
            table.insert(InConcat, tostring(V))
        elseif type(V) == "string" then
            table.insert(InConcat, V == "" and "[empty string]" or V)
        elseif type(V) == "function" then
            table.insert(InConcat, "[function]")
        elseif type(V) == "table" then
            if Level >= MaxLevel then
                table.insert(InConcat, "{...}")
            else
                table2StrInner(V, Level + 1, InConcat, MaxLevel)
            end
        elseif type(V) == "userdata" then
            table.insert(InConcat, "[userdata]")
        else
            table.insert(InConcat, "[undefine]")
        end
        table.insert(InConcat, "\n")
    end
    doSpace(InConcat, Level-1)
    table.insert(InConcat,"}\n")
 end

 function table.tostring_block(Table, MaxLevel)
	if nil == Table then
		return "nil"
	end

    MaxLevel = MaxLevel or 2
    local Concat = {}
    table2StrInner(Table,1,Concat,MaxLevel)
    return table.concat(Concat)
 end