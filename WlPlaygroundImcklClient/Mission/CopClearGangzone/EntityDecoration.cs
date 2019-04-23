using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


/// <summary>
/// Ported from: https://github.com/thers/FRFuel
/// </summary>
namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    public enum DecorationType
    {
        Float = 1,
        Bool = 2,
        Int = 3,
        Time = 5
    }

    public static class EntityDecoration
    {
        internal static Type floatType = typeof(float);
        internal static Type boolType = typeof(bool);
        internal static Type intType = typeof(int);

        public static bool ExistOn(Entity entity, string propertyName)
        {
            return API.DecorExistOn(entity.Handle, propertyName);
        }

        public static bool HasDecor(this Entity ent, string propertyName)
        {
            return ExistOn(ent, propertyName);
        }

        public static void RegisterProperty(string propertyName, DecorationType type)
        {
            API.DecorRegister(propertyName, (int)type);
        }

        public static void Set(Entity entity, string propertyName, float floatValue)
        {
            API.DecorSetFloat(entity.Handle, propertyName, floatValue);
        }

        public static void Set(Entity entity, string propertyName, int intValue)
        {
            API.DecorSetInt(entity.Handle, propertyName, intValue);
        }

        public static void Set(Entity entity, string propertyName, bool boolValue)
        {
            API.DecorSetBool(entity.Handle, propertyName, boolValue);
        }

        public static void SetDecor(this Entity ent, string propertyName, float value)
        {
            Set(ent, propertyName, value);
        }

        public static void SetDecor(this Entity ent, string propertyName, int value)
        {
            Set(ent, propertyName, value);
        }

        public static void SetDecor(this Entity ent, string propertyName, bool value)
        {
            Set(ent, propertyName, value);
        }

        public static T Get<T>(Entity entity, string propertyName)
        {
            if (!ExistOn(entity, propertyName))
            {
                throw new EntityDecorationUnregisteredPropertyException();
            }

            Type genericType = typeof(T);

            if (genericType == floatType)
            {
                return (T)(object)API.DecorGetFloat(entity.Handle, propertyName);
            }
            else if (genericType == intType)
            {
                return (T)(object)API.DecorGetInt(entity.Handle, propertyName);
            }
            else if (genericType == boolType)
            {
                return (T)(object)API.DecorGetBool(entity.Handle, propertyName);
            }
            else
            {
                throw new EntityDecorationUndefinedTypeException();
            }
        }

        public static T GetDecor<T>(this Entity ent, string propertyName)
        {
            return Get<T>(ent, propertyName);
        }
    }

    public class EntityDecorationUnregisteredPropertyException : Exception { }
    public class EntityDecorationUndefinedTypeException : Exception { }
}
