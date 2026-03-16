import Navbar from '../components/Navbar/Navbar';
import Hero from '../components/Hero/Hero';
import HowItWorks from '../components/HowItWorks/HowItWorks';
import DashboardPreview from '../components/DashboardPreview/DashboardPreview';
import Features from '../components/Features/Features';
import WaitlistCTA from '../components/WaitlistCTA/WaitlistCTA';
import Footer from '../components/Footer/Footer';

export default function LandingPage() {
  return (
    <>
      <Navbar />
      <main>
        <Hero />
        <HowItWorks />
        <DashboardPreview />
        <Features />
        <WaitlistCTA />
      </main>
      <Footer />
    </>
  );
}
