﻿using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.OpenGL
{
    class Sync
    {
        private class SyncHandle
        {
            public ulong ID;
            public IntPtr Handle;
        }

        private ulong _firstHandle = 0;

        private List<SyncHandle> Handles = new List<SyncHandle>();

        public void Create(ulong id)
        {
            SyncHandle handle = new SyncHandle
            {
                ID = id,
                Handle = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None)
            };

            lock (Handles)
            {
                Handles.Add(handle);
            }
        }

        public void Wait(ulong id)
        {
            SyncHandle result = null;

            lock (Handles)
            {
                if ((long)(_firstHandle - id) > 0)
                {
                    return; // The handle has already been signalled or deleted.
                }

                foreach (SyncHandle handle in Handles)
                {
                    if (handle.ID == id)
                    {
                        result = handle;
                        break;
                    }
                }
            }

            if (result != null)
            {
                lock (result)
                {
                    if (result.Handle == IntPtr.Zero)
                    {
                        return;
                    }

                    WaitSyncStatus syncResult = GL.ClientWaitSync(result.Handle, ClientWaitSyncFlags.SyncFlushCommandsBit, 1000000000);
                    
                    if (syncResult == WaitSyncStatus.TimeoutExpired)
                    {
                        Logger.Error?.PrintMsg(LogClass.Gpu, $"GL Sync Object {result.ID} failed to signal within 1000ms. Continuing...");
                    }
                }
            }
        }

        public void Cleanup()
        {
            // Iterate through handles and remove any that have already been signalled.

            while (true)
            {
                SyncHandle first = null;
                lock (Handles)
                {
                    first = Handles.FirstOrDefault();
                }

                if (first == null) break;

                WaitSyncStatus syncResult = GL.ClientWaitSync(first.Handle, ClientWaitSyncFlags.SyncFlushCommandsBit, 0);

                if (syncResult == WaitSyncStatus.AlreadySignaled)
                {
                    // Delete the sync object.
                    lock (first)
                    {
                        _firstHandle = first.ID + 1;
                        Handles.RemoveAt(0);
                        GL.DeleteSync(first.Handle);
                        first.Handle = IntPtr.Zero;
                    }
                } else
                {
                    // This sync handle and any following have not been reached yet.
                    break;
                }
            }
        }
    }
}
