---
--- Created by Z_End.
--- DateTime: 2025/3/7 23:19
---

---@alias ObCardActionDelegate fun(rec:InGameCardBridge,give:(InGameCardBridge|nil)

---@class Events
---@field RegModLoadComplete fun(self:Events,action:fun())
---@field RegOnAction fun(self:Events,key:string,action:ObCardActionDelegate))
---@field RegOnEndAction fun(self:Events,key:string,action:ObCardActionDelegate))
Events = {}

---@class GameBridge
---@field [string] (UniqueIdObjectBridge|nil)
Game = {}

---@class UniqueIdObjectBridge
---@field __index(string) number
UniqueIdObjectBridge = {}

---@class InGameCardBridge
---@field AddCard fun(self:InGameCardBridge,obj:UniqueIdObjectBridge)
---@field Delete fun(self:InGameCardBridge)
---@field ResetInventory fun(self:InGameCardBridge)
---@field InventoryCardsCount number
---@field __index fun(self:InGameCardBridge,idx:number):InGameCardBridge[]
---@field __index fun(self:InGameCardBridge,key:string):number
---@field __newindex fun(self:InGameCardBridge,key:string,val:number)
InGameCardBridge = {}
