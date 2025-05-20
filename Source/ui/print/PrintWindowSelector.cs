using ArchotechInfusions.comps;
using Verse;

namespace ArchotechInfusions.ui.print;

public class PrintWindowSelector(Comp_Printer comp)
{
    private ThingSelectorWindow _thingSelectorWindow;
    private PrintWindow _printWindow;

    public void OpenThingSelector(Pawn pawn)
    {
        if (_thingSelectorWindow != null || _printWindow != null)
        {
            Log.Error("JAI: Already opened printing window");
            Find.WindowStack.TryRemove(typeof(PrintWindowSelector));
            Find.WindowStack.TryRemove(typeof(ThingSelectorWindow));
        }

        _thingSelectorWindow = new ThingSelectorWindow(this, pawn, comp);
        Find.WindowStack.Add(_thingSelectorWindow);
    }


    public void ForceClose()
    {
        Find.WindowStack.TryRemove(typeof(ThingSelectorWindow));
        Find.WindowStack.TryRemove(typeof(PrintWindow));
    }

    public void OnThingSelected(Pawn pawn, Thing thing)
    {
        Find.WindowStack.TryRemove(typeof(ThingSelectorWindow));
        _printWindow = new PrintWindow(this, comp, thing);
        Find.WindowStack.Add(_printWindow);
    }

    public void OnThingSelectorWindowClosed()
    {
        _thingSelectorWindow = null;
    }

    public void OnPrintWindowClosed()
    {
        _printWindow = null;
    }
}