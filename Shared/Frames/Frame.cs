using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace izolabella.WebSocket.Unity.Shared.Frames
{
#nullable enable
    public class Frame
    {
        public Frame(HandlerRequestModel Model)
        {
            this.Model = Model;
            this.SizeBytes = Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(this.Model)) + 4;
        }

        public HandlerRequestModel Model { get; }

        /// <summary>
        /// The size, in bytes, that this request will be in total.
        /// 4 bytes + content size.
        /// </summary>
        public int SizeBytes { get; }

        /// <summary>
        /// Creates a <see cref="Frame"/> from the byte equivalent. Set <paramref name="Skip4"/> to true
        /// if <paramref name="Data"/>'s first four bytes are the content size.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Skip4"></param>
        /// <returns></returns>
        public static Frame? FromBytes(byte[] Data, bool Skip4 = true)
        {
            byte[] JSONBytes = Skip4 ? Data.Skip(4).ToArray() : Data;
            string JSON = Encoding.UTF8.GetString(JSONBytes);
            HandlerRequestModel? F = JsonConvert.DeserializeObject<HandlerRequestModel>(JSON);
            return F != null ? (new(F)) : null;
        }

        /// <summary>
        /// first 4 bytes = int of size, rest is content
        /// </summary>
        /// <param name="Frame"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Frame Frame)
        {
            byte[] Data = new byte[Frame.SizeBytes];
            byte[] IntBytes = BitConverter.GetBytes(Frame.SizeBytes);
            Debug.Log(Frame.SizeBytes + " - full frame size");
            for (int Index = 0; Index < IntBytes.Length; Index++)
            {
                Data[Index] = IntBytes[Index];
            }
            string JSONDataString = JsonConvert.SerializeObject(Frame.Model);
            Debug.Log(JSONDataString);
            byte[] JSONData = Encoding.UTF8.GetBytes(JSONDataString);
            for (int Index = IntBytes.Length; Index < Data.Length; Index++)
            {
                Data[Index] = JSONData[Index - 4];
            }
            return Data;
        }
    }
}
