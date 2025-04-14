namespace MontefaroMatias
{
    public static class Common
    {
        public enum Orientation
        {
            North,
            South,
            East,
            West
        }
        public enum orderType:byte
        {
            toViaLibre = 0,
            toParada = 1,
            toAvisoDeParada = 2,
            toPrecaucion = 3,
            toRebaseAutorizado = 4,
            toUnknown=255
        }
        public enum layoutTraceStatus:byte
        {
            ltDisabled=0, //En gris, sin detectores.
            ltFree=1, //Amarillo. Libre de trenes.
            ltLocked=2, //Verde. Enclavado.
            ltShunt=3, //Azul. Enclavado para maniobras.
            ltOccupied=4,//Rojo. Ocupado.
            ltUnknown=255 //Error o estado desconocido.
        }
        public enum crossingStatus:byte
        {
            csDisabled=0, //Paso a nivel desactivado.
            csOpen=1,//Abierto al tren (cerrado a la carretera)
            csClosed=2,//Cerrado al tren (abierto a la carretera)
            csUnknown=255 //Estado desconocido
        }
    }
}
