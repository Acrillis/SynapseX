#pragma once

namespace syn::FakeMemcheck
{
    inline int MemoryChanged = 0;

    void UpdateMasterHash();
    void Initialize();
    bool MemChanged();
}