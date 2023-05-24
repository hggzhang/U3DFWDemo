local TableUtil = {}

_G.TableUtil = TableUtil

function TableUtil:RemoveItem(t, item)
    for key, val in pairs(t) do
        if val == item then
            t[key] = nil
            return true
        end
    end

    return false
end