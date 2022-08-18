using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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
        /// Creates a <see cref="Frame"/> from the byte equivalent.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static Frame? FromBytes(byte[] Data)
        {
            byte[] JSONBytes = Data.Skip(4).ToArray();
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
            for (int Index = 0; Index < IntBytes.Length; Index++)
            {
                Data[Index] = IntBytes[Index];
            }
            string JSONDataString = JsonConvert.SerializeObject(Frame.Model);
            byte[] JSONData = Encoding.UTF8.GetBytes(JSONDataString);
            for (int Index = IntBytes.Length; Index < Data.Length; Index++)
            {
                Data[Index] = JSONData[Index - 4];
            }
            return Data;
        }
    }
}
