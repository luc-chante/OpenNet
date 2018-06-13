namespace QuadriPlus.Extensions.Logging.File
{
    /// <summary>
    /// Comportement du logger par rapport à l'ancien fichier de log
    /// </summary>
    public enum FileLoggerBehaviour
    {
        /// <summary>
        /// Ajoute continuellement le texte au fichier de log
        /// </summary>
        Append = 0,

        /// <summary>
        /// Le fichier de log est systématiquemenet écrasé au démarrage de l'appli
        /// </summary>
        Override = 1,

        /// <summary>
        /// Le fichier de log précédent est sauvegardé avant création du nouveau
        /// </summary>
        Backup = 2
    }
}
