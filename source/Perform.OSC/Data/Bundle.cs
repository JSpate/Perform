using System.Text;

namespace Perform.OSC.Data;

public class Bundle : Packet
{
    private readonly Timestamp _timestamp;

    public Bundle(ulong timestamp, params Message[] args)
    {
        _timestamp = timestamp;
        Messages = new List<Message>();
        Messages.AddRange(args);
    }
		
    public new List<Message> Messages;

    public override byte[] GetBytes()
    {
        var bundle = "#bundle";
        var bundleTagLen = bundle.AlignedStringLength();
        var tag = SetULong(_timestamp.Tag);

        var outMessages = new List<byte[]>();
        foreach (var msg in Messages)
        {
            outMessages.Add(msg.GetBytes());
        }

        var len = bundleTagLen + tag.Length + outMessages.Sum(x => x.Length + 4);

        var i = 0;
        var output = new byte[len];
        Encoding.ASCII.GetBytes(bundle).CopyTo(output, i);
        i += bundleTagLen;
        tag.CopyTo(output, i);
        i += tag.Length;

        foreach (var msg in outMessages)
        {
            var size = SetInt(msg.Length);
            size.CopyTo(output, i);
            i += size.Length;

            msg.CopyTo(output, i);
            i += msg.Length; // msg size is always a multiple of 4
        }

        return output;
    }

}