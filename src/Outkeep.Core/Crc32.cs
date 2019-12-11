using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public class Crc32Stream : Stream
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;

        public Crc32Stream(Stream stream, bool leaveOpen = false)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // todo: update crc32 here
            return _stream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            // todo: update crc32 here
            return _stream.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // todo: update crc32 here
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // todo: update crc32 here
            return base.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            // todo: update crc32 here
            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // todo: update crc32 here
            _stream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // todo: update crc32 here
            _stream.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // todo: update crc32 here
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // todo: update crc32 here
            return _stream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            // todo: update crc32 here
            base.WriteByte(value);
        }

        public override int WriteTimeout
        {
            get => _stream.WriteTimeout;
            set => _stream.WriteTimeout = value;
        }

        public override int ReadTimeout
        {
            get => _stream.ReadTimeout;
            set => _stream.ReadTimeout = value;
        }

        public override bool CanTimeout => _stream.CanTimeout;

        public override void CopyTo(Stream destination, int bufferSize)
        {
            // todo: update crc32 here
            _stream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            // todo: update crc32 here
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // todo: update crc32 here
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // todo: update crc32 here
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            // todo: update crc32 here
            return _stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            // todo: update crc32 here
            _stream.EndWrite(asyncResult);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (!_leaveOpen)
            {
                await _stream.DisposeAsync();
            }

            await base.DisposeAsync();
        }
    }
}