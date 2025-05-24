using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stego.Core
{
    public record EncryptionEnvelope
    {
        public required byte[] Salt { get; init; }
        public required byte[] Nonce { get; init; }
        public required int Parallelism { get; init; }
        public required long MemorySize { get; init; }
        public required long Iterations { get; init; }
        public required byte[] EncryptedData { get; init; }
    }
}
