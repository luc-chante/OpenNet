using System;

namespace QuadriPlus.Extensions.Logging.File
{
    /// <summary>
    /// Détail du comportement suivant le choix de backup
    /// 
    /// Les valeurs de cet enum sont des flags et peuvent être mixés
    /// </summary>
    [Flags]
    public enum FileLoggerBackupMode
    {
        /// <summary>
        /// Par défaut on incrémente le numéro du fichier de backup au démarrage
        /// 
        /// Correspond à : FileLoggerBackupMode.Startup
        /// </summary>
        Default = 0,

        /// <summary>
        /// La sauvegarde est effectuée de manière systématique au lancement de l'appli
        /// </summary>
        Startup = 1,

        /// <summary>
        /// La sauvegarde est effectuée dès que le fichier de log dépasse une certaine taille
        /// </summary>
        Size = 2,

        /// <summary>
        /// La sauvegarde est effectuée quand le fichier a une ancienneté supérieure à celle précisée
        /// </summary>
        Age = 4
    }
}
