local VM = _G.UI.VM


local M = _G.Class(VM)

function M:Ctor()
    self.playerName = "Jack"
    self.gold = 10
    self.icon = "Assets/GameRes/Image/touxiang1.png"
    self.checkTips = "";
    self.loginTips = "";
end

function M:SetCheckTips(V)
    self.checkTips = V
end

function M:SetLoginTips(V)
    self.loginTips = V
end

function M:SetPlayerName(V)
    self.playerName = V
end

function M:SetGold(V)
    self.gold = V
end

return M