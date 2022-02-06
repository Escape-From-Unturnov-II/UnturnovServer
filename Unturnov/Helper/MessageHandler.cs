using Rocket.Core.Logging;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    internal class MessageHandler
    {
		public static void SendMessageToClient(EClientMessage index, ENetReliability reliability, ITransportConnection transportConnection, ClientWriteHandler callback)
		{
			if(writer == null)
            {
				Logger.LogError("writer of MessageHandler was not initialized!");
				return;
            }
			writer.Reset();
			writer.WriteEnum(index);
			callback(writer);
			writer.Flush();
			transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
		}

		public static void Init()
        {
			UnturnedPrivateFields.TryGetNetMessagesWriter(out writer);
		}

		internal static NetPakWriter writer;
		public delegate void ClientWriteHandler(NetPakWriter writer);
	}

}
