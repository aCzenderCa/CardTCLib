---
--- Created by Z_End.
--- DateTime: 2025/3/7 23:19
---

---@alias OnCardActionDelegate fun(rec:InGameCardBridge,give:(InGameCardBridge|nil)
---@alias BatchEnumeratorsActionDelegate fun(cache:List<IEnumerator>)

---@generic T
---@class List<T>
---@field Add fun(self:List<T>,val:T)

---@class IEnumerator

---@class Events
---@field RegModLoadComplete fun(self:Events,action:fun():void)
---@field RegModLoadCompletePost fun(self:Events,action:fun():void)
---@field RegOnAction fun(self:Events,key:string,action:OnCardActionDelegate))
---@field RegOnEndAction fun(self:Events,key:string,action:OnCardActionDelegate))
---@field RegCardNameOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(card:InGameCardBridge):string))
---@field RegCardDescOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(card:InGameCardBridge):string))
---@field RegActionNameOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(actionName:string,card:InGameCardBridge):string))
---@field RegActionTipOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(actionName:string,card:InGameCardBridge):string))
---@field RegPossibleActionOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(rec:InGameCardBridge,give:InGameCardBridge):int|nil))
---@field RegOnDisplayEncounterPlayerActions fun(self:Events,action:fun())
---@field RegOnPlayerEncounterAction fun(self:Events,id:string,action:fun())
Events = {}

---@class CoroutineHelper
---@field BatchEnumerators fun(self:CoroutineHelper,action:BatchEnumeratorsActionDelegate)
---@field PushCoQueue fun(self:CoroutineHelper)
CoroutineHelper = {}

---@alias FadeToBlackTypes ("None"|"Partial"|"Full")
---@alias CardActionType ("CardAction"|"CardOnCardAction"|"DismantleCardAction"|"FromStatChangeAction")
---@alias CardTypes ("Item"|"Base"|"Location"|"Event"|"Environment"|"Weather"|"Hand"|"Blueprint"|"Explorable"|"Liquid"|"EnvImprovement"|"EnvDamage"|"BlueprintInLocation"|"InvisibleCard")
---@class GameBridge
---@field CreateCardTag fun(self:GameBridge,tagId:string,tagName:string):CardTag
---@field CreateAction fun(self:GameBridge,id:string,name:string,type:CardActionType):CardActionBridge
---@field CreateCard fun(self:GameBridge,id:string,name:string,type:CardTypes,icon:string):UniqueIdObjectBridge
---@field CreateDurabilityStat fun(self:GameBridge,id:string,args:table,active:boolean|nil):DurabilityStat
---
---@field Log fun(self:GameBridge,message:string)
---@field GetGlobalValue fun(self:GameBridge,key:string):string|number|nil
---@field SetGlobalValue fun(self:GameBridge,key:string,val:string|number|nil)
---@field PassTime fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string)
---@field PassTimeEnum fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string):IEnumerator
---@field GetItem fun(self:GameBridge,idOrName:string):(UniqueIdObjectBridge|nil)
---@field CloseCurrentInspectionPopup fun(self:GameBridge)
---@field RefreshCurrentInspectionPopup fun(self:GameBridge)
---@field FindCards fun(self:GameBridge,cardData:UniqueIdObjectBridge,cache:InGameCardBridge[],includeBackground:boolean|nil):InGameCardBridge[]
---@field FindCardsByTag fun(self:GameBridge,tag:string,cache:InGameCardBridge[],includeBackground:boolean|nil):InGameCardBridge[]
---@field AddEncounterAction fun(self:GameBridge,id:string,name:string)
---@field DoDamageToEnemy fun(self:GameBridge,damage:number,change:number,hpScale:number|nil,moraleScale:number|nil)
---@field HealPlayer fun(self:GameBridge,heal:number,change:number,hpScale:number|nil,moraleScale:number|nil)
---@field MiniTicksPerTick number
Game = {}

---@class AddCardExtraArg
---@field Trans boolean
---
