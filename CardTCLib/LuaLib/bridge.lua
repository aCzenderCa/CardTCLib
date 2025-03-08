---
--- Created by Z_End.
--- DateTime: 2025/3/7 23:19
---

---@alias ObCardActionDelegate fun(rec:InGameCardBridge,give:(InGameCardBridge|nil)
---@alias BatchEnumeratorsActionDelegate fun(cache:List<IEnumerator>)

---@generic T
---@class List<T>
---@field Add fun(self:List<T>,val:T)

---@class IEnumerator

---@class Events
---@field RegModLoadComplete fun(self:Events,action:fun():void)
---@field RegOnAction fun(self:Events,key:string,action:ObCardActionDelegate))
---@field RegOnEndAction fun(self:Events,key:string,action:ObCardActionDelegate))
Events = {}

---@class CoroutineHelper
---@field BatchEnumerators fun(self:CoroutineHelper,action:BatchEnumeratorsActionDelegate)
CoroutineHelper = {}

---@alias FadeToBlackTypes ("None"|"Partial"|"Full")
---@alias CardActionType ("CardAction"|"CardOnCardAction"|"DismantleCardAction"|"FromStatChangeAction")
---@class GameBridge
---@field CreateAction fun(self:GameBridge,id:string,name:string,type:CardActionType):CardActionBridge
---@field Log fun(self:GameBridge,message:string)
---@field PassTime fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string)
---@field PassTimeEnum fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string):IEnumerator
---@field GetItem fun(self:GameBridge,key:string):(UniqueIdObjectBridge|nil)
---@field MiniTicksPerTick number
Game = {}

---@class CardActionBridge

---@class UniqueIdObjectBridge
---@field GenNpcCardModel fun(self:UniqueIdObjectBridge):UniqueIdObjectBridge
---@field AddAction fun(self:UniqueIdObjectBridge,action:CardActionBridge)
---@field RemoveAction fun(self:UniqueIdObjectBridge,type:CardActionType,id:string)
---@field GetStatInfo fun(self:UniqueIdObjectBridge):number,number,number,number
---@field SetStat fun(self:UniqueIdObjectBridge,newValue:number,newRate:number)
---@field SetStatEnum fun(self:UniqueIdObjectBridge,newValue:number,newRate:number):IEnumerator
---@field AddStat fun(self:UniqueIdObjectBridge,newValue:number,newRate:number)
---@field AddStatEnum fun(self:UniqueIdObjectBridge,newValue:number,newRate:number):IEnumerator
---@field Name string
---@field NameLocal string
---@field NameChinese string
---@field GetItem fun(self:UniqueIdObjectBridge,key:string):number
---@field SetItem fun(self:UniqueIdObjectBridge,key:string):number
UniqueIdObjectBridge = {}

---@class InGameCardBridge
---@field AddCard fun(self:InGameCardBridge,obj:UniqueIdObjectBridge)
---@field AddCardEnum fun(self:InGameCardBridge,obj:UniqueIdObjectBridge):IEnumerator
---@field Delete fun(self:InGameCardBridge)
---@field DeleteEnum fun(self:InGameCardBridge):IEnumerator
---@field ResetInventory fun(self:InGameCardBridge)
---@field ResetInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field ClearInventory fun(self:InGameCardBridge)
---@field ClearInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field InventoryCardsCount number
---@field get_Item fun(self:InGameCardBridge,idx:number):InGameCardBridge[]
---@field get_Item fun(self:InGameCardBridge,key:string):number
---@field set_Item fun(self:InGameCardBridge,key:string,val:number)
InGameCardBridge = {}
