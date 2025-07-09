using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class LogoPreloader
    {
        public void PrintStrokesToConsole(Stroke[] _strokes)
        {
            System.Diagnostics.Debug.WriteLine("var logoStrokes = new List<Stroke>();");

            foreach (var stroke in _strokes)
            {
                var color = (int)stroke.Color;
                var size = (int)stroke.Size;
                var cap = stroke.PenLineCap;
                var alpha = stroke.Alpha.ToString("0.##");
                var shake = stroke.Shake ? "true" : "false";

                System.Diagnostics.Debug.WriteLine(
                    $"var stroke = new Stroke((ColorType){color}, new Point({stroke.Points[0].X}, {stroke.Points[0].Y}), (SizeType){size}, {alpha}, PenLineCap.{cap}, {stroke.Pressures[0]:0.##f}, {shake});");

                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    var p = stroke.Points[i];
                    var pressure = stroke.Pressures[i];
                    System.Diagnostics.Debug.WriteLine($"stroke.Points.Add(new Point({p.X}, {p.Y}));");
                    System.Diagnostics.Debug.WriteLine($"stroke.Pressures.Add({pressure:0.##f});");
                }

                System.Diagnostics.Debug.WriteLine($"logoStrokes.Add(stroke);");
                System.Diagnostics.Debug.WriteLine(""); // spacing
            }

            System.Diagnostics.Debug.WriteLine("return logoStrokes;");
        }
        public void LoadPredefinedStrokes(FrameController frameController, Control control)
        {
            var logoStrokes = LogoStrokes.Get();
            var clonedStrokes = logoStrokes.Select(s => s.Clone()).ToList();

            frameController.ClearAll();
            frameController.AddEmptyFrame();
            frameController.SetStrokes(clonedStrokes);
            frameController.SyncStrokesToFrame();
            control.InvalidateVisual();
        }

    }
    public static class LogoStrokes
    {
        public static List<Stroke> Get()
        {
            var logoStrokes = new List<Stroke>();
            var stroke = new Stroke(0, new Point(37.067171891147154, 10.949689000866101), 0, 1, PenLineCap.Square, 0.01f, true);
            stroke.Points.Add(new Point(32.21701687909058, 12.310211794346912));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(27.14640027557698, 15.59814187859223));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(22.516706854977656, 18.886071962837548));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(18.548398208749518, 22.4007558459964));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(23.61901481226323, 24.328163136760907));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(28.46916982431958, 21.720494449255966));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(33.319324836376154, 19.79308715849146));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(38.38994143988975, 18.999448862294287));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(42.79917326903205, 21.720494449255966));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(43.46055804340335, 26.93583182426579));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(41.2559421288322, 31.811038500905454));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(37.728556665518454, 35.55247618297773));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(33.319324836376154, 37.93339107156919));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(28.68963141577683, 39.974175261790435));
            stroke.Pressures.Add(1f);
            stroke.Points.Add(new Point(23.839476403720255, 41.674828753641464));
            stroke.Pressures.Add(1f);
            logoStrokes.Add(stroke);

            var stroke1 = new Stroke(0, new Point(52.499483293144976, 36.7996220770018), 0, 1, PenLineCap.Square, 0.03f, true);
            stroke1.Points.Add(new Point(51.61763692731665, 31.811038500905454));
            stroke1.Pressures.Add(1f);
            stroke1.Points.Add(new Point(51.61763692731665, 26.255570427525413));
            stroke1.Pressures.Add(1f);
            stroke1.Points.Add(new Point(51.17671374440238, 21.040233052515532));
            stroke1.Pressures.Add(1f);
            stroke1.Points.Add(new Point(50.51532897003108, 16.051649476419186));
            stroke1.Pressures.Add(1f);
            stroke1.Points.Add(new Point(49.19255942128825, 11.176442799779522));
            stroke1.Pressures.Add(1f);
            logoStrokes.Add(stroke1);

            var stroke2 = new Stroke(0, new Point(53.160868067516276, 37.479883473742234), 0, 1, PenLineCap.Square, 0f, true);
            stroke2.Points.Add(new Point(52.499483293144976, 32.37792299818909));
            stroke2.Pressures.Add(1f);
            stroke2.Points.Add(new Point(53.60179125043055, 27.162585623179268));
            stroke2.Pressures.Add(1f);
            stroke2.Points.Add(new Point(57.1291767137443, 23.534524840563734));
            stroke2.Pressures.Add(1f);
            stroke2.Points.Add(new Point(60.8770237685153, 27.04920872372253));
            stroke2.Pressures.Add(1f);
            stroke2.Points.Add(new Point(61.75887013434385, 32.15116919927567));
            stroke2.Pressures.Add(1f);
            stroke2.Points.Add(new Point(61.979331725800876, 37.253129674828756));
            stroke2.Pressures.Add(1f);
            logoStrokes.Add(stroke2);

            var stroke3 = new Stroke(0, new Point(78.07302790217022, 27.275962522636007), 0, 1, PenLineCap.Square, 0.01f, true);
            stroke3.Points.Add(new Point(73.22287289011365, 25.915439729155196));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(68.59317946951433, 28.29635461774666));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(67.27040992077173, 33.284938193843004));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(69.47502583534265, 37.93339107156919));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(74.54564243885625, 37.479883473742234));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(78.29348949362725, 34.07857649004018));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(81.1594901825697, 29.77025431068421));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(84.02549087151215, 34.07857649004018));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(88.21426110919742, 37.13975277537202));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.28487771271102, 36.346114479174844));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.50533930416805, 31.24415400362176));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.50533930416805, 25.915439729155196));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(92.84395452979675, 20.926856153058793));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(92.62349293833972, 15.71151877804897));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.06441612125377, 21.040233052515532));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.50533930416805, 26.028816628611935));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(98.35549431622462, 27.389339422092746));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(101.4419565966241, 23.307771041650255));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(97.47364795039607, 26.82245492480905));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(93.50533930416805, 29.883631210140948));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(98.35549431622462, 31.697661601448715));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(102.98518773682395, 33.96519959058344));
            stroke3.Pressures.Add(1f);
            stroke3.Points.Add(new Point(107.61488115742327, 36.11936068026142));
            stroke3.Pressures.Add(1f);
            logoStrokes.Add(stroke3);

            var stroke4 = new Stroke(0, new Point(111.5831898036513, 24.895047634044545), 0, 1, PenLineCap.Square, 0.02f, true);
            stroke4.Points.Add(new Point(111.36272821219427, 30.110385009054426));
            stroke4.Pressures.Add(0.93f);
            stroke4.Points.Add(new Point(114.0082673096797, 34.645460987323816));
            stroke4.Pressures.Add(0.96f);
            stroke4.Points.Add(new Point(119.0788839131933, 34.3053302889536));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(121.28349982776444, 29.54350051177073));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(121.94488460213574, 24.328163136760907));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(124.1495005167069, 28.863239115030296));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(127.67688598002064, 32.71805369655931));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(131.42473303479164, 36.45949137863158));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(133.84981054081982, 41.10794425635777));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(135.17258008956242, 46.096527832454115));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(135.39304168101967, 51.198488308007256));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(133.40888735790554, 55.960318085190124));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(128.99965552876324, 58.90811747106528));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(124.1495005167069, 60.155263365089354));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(119.29934550465032, 58.567986772695065));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(119.29934550465032, 53.46602629714198));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(123.04719255942132, 50.064719313439866));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(128.11780916293492, 48.2506889221321));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(133.40888735790554, 47.230296827021505));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(138.47950396141914, 46.096527832454115));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(143.55012056493274, 44.05574364223287));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(147.95935239407504, 40.88119045744429));
            stroke4.Pressures.Add(1f);
            stroke4.Points.Add(new Point(151.04581467447474, 36.7996220770018));
            stroke4.Pressures.Add(1f);
            logoStrokes.Add(stroke4);

            var stroke5 = new Stroke(0, new Point(19.43024457457807, 91.33391071569167), 0, 1, PenLineCap.Square, 0.01f, true);
            stroke5.Points.Add(new Point(20.753014123320668, 86.11857334068179));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(20.532552531863644, 81.01661286512876));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(19.650706166035093, 75.68789859066214));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(18.548398208749518, 70.585938115109));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(17.22562866000692, 65.71073143846945));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(16.34378229417848, 60.72214786237305));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(21.41439889769208, 58.90811747106528));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(26.48501550120568, 59.2482481694355));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(31.55563210471928, 60.49539406345957));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(36.1853255253186, 62.762932052594294));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(39.933172580089604, 66.27761593575315));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(42.35825008611778, 70.69931501456574));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(42.79917326903205, 75.68789859066214));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(41.476403720289454, 80.56310526730181));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(37.94901825697548, 84.53129674828756));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(33.76024801929043, 87.47909613416266));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(28.68963141577683, 89.17974962601375));
            stroke5.Pressures.Add(1f);
            stroke5.Points.Add(new Point(23.61901481226323, 88.95299582710027));
            stroke5.Pressures.Add(1f);
            logoStrokes.Add(stroke5);

            var stroke6 = new Stroke(0, new Point(57.570099896658576, 73.53373750098422), 0, 1, PenLineCap.Square, 0.01f, true);
            stroke6.Points.Add(new Point(52.499483293144976, 74.1006219982678));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(48.31071305545993, 77.16179828359975));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(45.6651739579745, 81.47012046295572));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(46.3265587323458, 86.57208093850875));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(50.956252152945126, 88.83961892764353));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(56.026868756458725, 87.81922683253288));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(58.8928694454014, 83.39752775372017));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(60.215638994144, 78.5223210770805));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(57.79056148811583, 74.1006219982678));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(60.65656217705828, 69.79229981891194));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(65.2862555976576, 67.86489252814738));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(70.13641060971418, 69.56554601999846));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(73.66379607302792, 73.30698370207074));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(74.54564243885625, 78.29556727816703));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(73.00241129865663, 83.05739705534995));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(69.0341026524286, 86.11857334068179));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(63.963486048915, 86.11857334068179));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(59.99517740268698, 82.83064325643647));
            stroke6.Pressures.Add(1f);
            stroke6.Points.Add(new Point(58.67240785394415, 77.84205968034018));
            stroke6.Pressures.Add(1f);
            logoStrokes.Add(stroke6);

            var stroke7 = new Stroke(0, new Point(91.3007233895969, 70.24580741673878), 0, 1, PenLineCap.Square, 0.02f, true);
            stroke7.Points.Add(new Point(86.2301067860833, 68.77190772380129));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(81.1594901825697, 68.99866152271477));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(76.7502583534274, 71.94646090858987));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(76.7502583534274, 77.04842138414301));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(79.1753358594558, 81.47012046295572));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(83.80502928005512, 84.41791984883082));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(88.87564588356872, 85.211558145028));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(93.7258008956253, 83.96441225100386));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(96.15087840165347, 79.42933627273442));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(94.82810885291087, 74.21399889772454));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(93.06441612125377, 68.88528462325803));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(91.52118498105415, 64.1234548460751));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(90.19841543231132, 58.79474057160854));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(89.09610747502575, 53.6927800960554));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(88.21426110919742, 48.704196519959055));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(88.6551842921117, 53.91953389496888));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(89.9779538408543, 59.02149437052202));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(91.52118498105415, 64.01007794661837));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(92.40303134688247, 69.22541532162825));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(93.7258008956253, 74.21399889772454));
            stroke7.Pressures.Add(1f);
            stroke7.Points.Add(new Point(94.3871856699966, 79.20258247382094));
            stroke7.Pressures.Add(1f);
            logoStrokes.Add(stroke7);

            var stroke8 = new Stroke(0, new Point(105.63072683430937, 84.19116604991734), 0, 1, PenLineCap.Square, 0.02f, true);
            stroke8.Points.Add(new Point(105.41026524285212, 79.0892055743642));
            stroke8.Pressures.Add(1f);
            stroke8.Points.Add(new Point(104.5284188770238, 73.87386819935443));
            stroke8.Pressures.Add(1f);
            stroke8.Points.Add(new Point(103.64657251119525, 68.65853082434455));
            stroke8.Pressures.Add(1f);
            stroke8.Points.Add(new Point(102.32380296245265, 62.98968585150777));
            stroke8.Pressures.Add(1f);
            stroke8.Points.Add(new Point(101.00103341371005, 57.66097157704115));
            stroke8.Pressures.Add(1f);
            logoStrokes.Add(stroke8);

            var stroke9 = new Stroke(0, new Point(116.21288322425085, 79.54271317219116), 0, 1, PenLineCap.Square, 0.02f, true);
            stroke9.Points.Add(new Point(121.0630382363072, 77.84205968034018));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(125.4722700654495, 75.23439099283519));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(125.91319324836377, 70.13243051728216));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(120.84257664485017, 68.54515392488781));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(116.21288322425085, 71.03944571293596));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(113.12642094385114, 75.00763719392171));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(111.80365139510855, 79.99622077001811));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(114.44919049259397, 84.53129674828756));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(119.51980709610757, 86.34532713959527));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(124.59042369962117, 85.55168884339821));
            stroke9.Pressures.Add(1f);
            stroke9.Points.Add(new Point(129.44057871167752, 83.51090465317691));
            stroke9.Pressures.Add(1f);
            logoStrokes.Add(stroke9);

            var stroke10 = new Stroke(ColorType.Third, new Point(156.11643127798834, 85.09818124557125), 0, 1, PenLineCap.Round, 0.01f, true);
            stroke10.Points.Add(new Point(151.92766104030306, 82.0370049602393));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(148.40027557698932, 78.06881347925355));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(144.87289011367557, 74.32737579718128));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(142.44781260764717, 69.90567671836868));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(145.31381329658984, 65.71073143846945));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(150.38442990010344, 64.91709314227228));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(154.7936617292455, 67.86489252814738));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(157.6596624181882, 72.05983780804661));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(160.74612469858766, 67.97826942760412));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(164.49397175335866, 64.35020864498858));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(168.6827419910437, 61.289032359656744));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(173.7533585945573, 61.74253995748364));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(174.4147433689286, 66.84450043303673));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(171.98966586290044, 71.37957641130618));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(168.6827419910437, 75.34776789229193));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(165.15535652772996, 79.0892055743642));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(161.18704788150194, 82.37713565860952));
            stroke10.Pressures.Add(1f);
            stroke10.Points.Add(new Point(156.77781605235964, 84.75805054720104));
            stroke10.Pressures.Add(1f);
            logoStrokes.Add(stroke10);

            return logoStrokes;
        }
    }
}
