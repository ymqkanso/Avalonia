// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Input;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Presenters
{
    public interface IItemsPresenter : IPresenter, ILogical
    {
        IPanel Panel { get; }

        void ScrollIntoView(object item);

        bool TryMoveFocus(NavigationDirection direction);
    }
}
