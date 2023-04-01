using Sora.Entities;
using SoraBot.Model;
using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.BLL
{
    public class YomeRandomBll
    {
        public static async Task<Darling> GetDarlingRomdunDay(string qq)
        {
            var day = DateTime.Now.ToString("dd");

            Darling DarlingRomdunDay = new();
            DarlingUser DarlingDayUser =await GetDayUserISAsync(qq);

            //判断是新用户还是旧用户
            //新用户插入
            //旧用户判断是否今日抽取
            if (DarlingDayUser!=null)
            {
                if (day == DarlingDayUser.day)
                {
                    DarlingRomdunDay = await GetDarlingLeftUserListAsync(qq);
                }
                else
                {
                    DarlingRomdunDay = await GetDarlingRomdunDayListAsync();
                    DarlingUser darlingUser = new()
                    {
                        DarlingID = DarlingRomdunDay.id,
                        day = day,
                        qq = qq
                    };
                    await SetDarlingUserDayUpAsync(darlingUser,qq);
                }
            }
            else
            {
                DarlingRomdunDay = await GetDarlingRomdunDayListAsync();

                DarlingUser darlingUser= new() { 
                    DarlingID=DarlingRomdunDay.id,
                    day=day,
                    qq=qq
                };
                await SetDarlingUserDayInAsync(darlingUser);
                
            }
            return DarlingRomdunDay;
        }

        public static async Task<DarlingUser> GetDayUserISAsync(string qq) =>
            await DbScoped.Sugar.Queryable<DarlingUser>().Where(x => x.qq == qq).FirstAsync();

        public static async Task<Darling> GetDarlingRomdunDayListAsync()=>
            await DbScoped.Sugar.Queryable<Darling>()
            .OrderBy(x => SqlFunc.GetRandom())
            .Take(1)
            .FirstAsync();

        public static async Task<Darling> GetDarlingLeftUserListAsync(string qq) =>
            await DbScoped.Sugar.Queryable<Darling>()
            .LeftJoin<DarlingUser>((D, user) => D.id == user.DarlingID)
            .Where((D, user) => user.qq == qq)
            .FirstAsync();

        public static async Task SetDarlingUserDayInAsync(DarlingUser darlingUser) =>
            await DbScoped.Sugar.Insertable(darlingUser)
            .ExecuteCommandAsync();

        public static async Task SetDarlingUserDayUpAsync(DarlingUser darlingUser,string qq)=>
            await DbScoped.Sugar.Updateable(darlingUser)
            .Where(x => x.qq == qq)
            .ExecuteCommandAsync();
    }
}
