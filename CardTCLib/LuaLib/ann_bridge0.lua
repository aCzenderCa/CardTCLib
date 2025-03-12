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
---@field RegModLoadCompletePost fun(self:Events,action:fun():void)
---@field RegOnAction fun(self:Events,key:string,action:ObCardActionDelegate))
---@field RegOnEndAction fun(self:Events,key:string,action:ObCardActionDelegate))
Events = {}

---@class CoroutineHelper
---@field BatchEnumerators fun(self:CoroutineHelper,action:BatchEnumeratorsActionDelegate)
CoroutineHelper = {}

---@alias FadeToBlackTypes ("None"|"Partial"|"Full")
---@alias CardActionType ("CardAction"|"CardOnCardAction"|"DismantleCardAction"|"FromStatChangeAction")
---@alias CardTypes ("Item"|"Base"|"Location"|"Event"|"Environment"|"Weather"|"Hand"|"Blueprint"|"Explorable"|"Liquid"|"EnvImprovement"|"EnvDamage"|"BlueprintInLocation"|"InvisibleCard")
---@class GameBridge
---@field CreateAction fun(self:GameBridge,id:string,name:string,type:CardActionType):CardActionBridge
---@field CreateCard fun(self:GameBridge,id:string,name:string,type:CardTypes,icon:string):UniqueIdObjectBridge
---@field Log fun(self:GameBridge,message:string)
---@field PassTime fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string)
---@field PassTimeEnum fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string):IEnumerator
---@field GetItem fun(self:GameBridge,key:string):(UniqueIdObjectBridge|nil)
---@field MiniTicksPerTick number
Game = {}

---@class CardActionBridge

---@class UniqueIdObjectBridge
---@field GetTValue fun(self:UniqueIdObjectBridge,key:string):number
---@field SetTValue fun(self:UniqueIdObjectBridge,key:string,val:number)
---
---@field SetBpResults fun(self:UniqueIdObjectBridge,data:CountedCard[])
---@field SetBpStage fun(self:UniqueIdObjectBridge,idx:number,data:BpStageEle[])
---@field SetBpTab fun(self:UniqueIdObjectBridge,mainTab:string,subTab:string)
---
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
---@field SetIcon fun(self:UniqueIdObjectBridge,icon:string)
---@field GetItem fun(self:UniqueIdObjectBridge,key:string):number
---@field SetItem fun(self:UniqueIdObjectBridge,key:string):number
---@field Weight number
---@field MaxWeightCapacity number
---@field AddSlot fun(self:UniqueIdObjectBridge,add:UniqueIdObjectBridge|nil)
---@field RemoveSlot fun(self:UniqueIdObjectBridge,idx:number)

---@class InGameCardBridge
---@field SetExtraValue fun(self:InGameCardBridge,key:string,val:string|number)
---@field GetExtraValue fun(self:InGameCardBridge,key:string):string|number|nil
---@field AddCard fun(self:InGameCardBridge,obj:UniqueIdObjectBridge)
---@field AddCardEnum fun(self:InGameCardBridge,obj:UniqueIdObjectBridge):IEnumerator
---@field Delete fun(self:InGameCardBridge)
---@field DeleteEnum fun(self:InGameCardBridge):IEnumerator
---@field ResetInventory fun(self:InGameCardBridge)
---@field ResetInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field ClearInventory fun(self:InGameCardBridge)
---@field ClearInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field TransformTo fun(self:InGameCardBridge,target:UniqueIdObjectBridge)
---@field TransformToEnum fun(self:InGameCardBridge,target:UniqueIdObjectBridge):IEnumerator
---@field InventoryCardsCount number
---@field HasTag fun(self:InGameCardBridge,tag:string):boolean
---@field GetItem fun(self:InGameCardBridge,idx:number):InGameCardBridge[]
---@field GetDurability fun(self:InGameCardBridge,key:string):number
---@field SetDurability fun(self:InGameCardBridge,key:string,val:number)
---@field SetDurabilityEnum fun(self:InGameCardBridge,key:string,val:number):IEnumerator
---@field AddDurability fun(self:InGameCardBridge,key:string,val:number)
---@field AddDurabilityEnum fun(self:InGameCardBridge,key:string,val:number):IEnumerator
