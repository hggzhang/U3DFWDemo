local M = {}
local CSUIUtil = _G.CS.UIUtil

function M.GetListViewCS(view, path)
    local csView = view.csView
    return CSUIUtil.GetListView(csView, path)
end

return M