
local Game = {}
_G.Game = Game

Game.Numeric = {}
Game.Numeric.NumericDefine = require "Game/Numeric/NumericDefine"
Game.Numeric.NumericUtil = require "Game/Numeric/NumericUtil"
Game.Numeric.Numeric = require "Game/Numeric/Numeric"
Game.Numeric.NumericMod = require "Game/Numeric/NumericMod"

Game.Entity = {}
Game.Entity.Entity = require "Game/Entity/Entity"
Game.Entity.Comp = require "Game/Entity/Comp"

Game.VMConfig = require "Game/VMConfig"
