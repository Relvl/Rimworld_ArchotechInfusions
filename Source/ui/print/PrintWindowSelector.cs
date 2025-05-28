using ArchotechInfusions.building;
using Verse;

namespace ArchotechInfusions.ui.print;

public class PrintWindowSelector(Printer printer)
{
    private PrintWindow _printWindow;
    private ThingSelectorWindow _thingSelectorWindow;

    public void OpenThingSelector(Pawn pawn)
    {
        if (_thingSelectorWindow != null || _printWindow != null)
        {
            Log.Error("JAI: Already opened printing window");
            Find.WindowStack.TryRemove(typeof(PrintWindowSelector));
            Find.WindowStack.TryRemove(typeof(ThingSelectorWindow));
        }

        _thingSelectorWindow = new ThingSelectorWindow(this, pawn, printer);
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
        _printWindow = new PrintWindow(this, printer, thing);
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