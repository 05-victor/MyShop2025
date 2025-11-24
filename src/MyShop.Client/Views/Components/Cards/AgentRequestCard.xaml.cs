using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace MyShop.Client.Views.Components.Cards
{
    public sealed partial class AgentRequestCard : UserControl
    {
        public AgentRequestCard()
        {
            this.InitializeComponent();
        }

        // Dependency Properties for binding
        public static readonly DependencyProperty FullNameProperty =
            DependencyProperty.Register(nameof(FullName), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(nameof(Email), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register(nameof(Avatar), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(AgentRequestCard), new PropertyMetadata("Pending"));

        public static readonly DependencyProperty SubmittedDateProperty =
            DependencyProperty.Register(nameof(SubmittedDate), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ReasonProperty =
            DependencyProperty.Register(nameof(Reason), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ExperienceProperty =
            DependencyProperty.Register(nameof(Experience), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RejectionReasonProperty =
            DependencyProperty.Register(nameof(RejectionReason), typeof(string), typeof(AgentRequestCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HasRejectionReasonProperty =
            DependencyProperty.Register(nameof(HasRejectionReason), typeof(Visibility), typeof(AgentRequestCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty IsPendingProperty =
            DependencyProperty.Register(nameof(IsPending), typeof(Visibility), typeof(AgentRequestCard), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ApproveCommandProperty =
            DependencyProperty.Register(nameof(ApproveCommand), typeof(ICommand), typeof(AgentRequestCard), new PropertyMetadata(null));

        public static readonly DependencyProperty RejectCommandProperty =
            DependencyProperty.Register(nameof(RejectCommand), typeof(ICommand), typeof(AgentRequestCard), new PropertyMetadata(null));

        public string FullName
        {
            get => (string)GetValue(FullNameProperty);
            set => SetValue(FullNameProperty, value);
        }

        public string Email
        {
            get => (string)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        public string Avatar
        {
            get => (string)GetValue(AvatarProperty);
            set => SetValue(AvatarProperty, value);
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public string SubmittedDate
        {
            get => (string)GetValue(SubmittedDateProperty);
            set => SetValue(SubmittedDateProperty, value);
        }

        public string Reason
        {
            get => (string)GetValue(ReasonProperty);
            set => SetValue(ReasonProperty, value);
        }

        public string Experience
        {
            get => (string)GetValue(ExperienceProperty);
            set => SetValue(ExperienceProperty, value);
        }

        public string RejectionReason
        {
            get => (string)GetValue(RejectionReasonProperty);
            set => SetValue(RejectionReasonProperty, value);
        }

        public Visibility HasRejectionReason
        {
            get => (Visibility)GetValue(HasRejectionReasonProperty);
            set => SetValue(HasRejectionReasonProperty, value);
        }

        public Visibility IsPending
        {
            get => (Visibility)GetValue(IsPendingProperty);
            set => SetValue(IsPendingProperty, value);
        }

        public ICommand ApproveCommand
        {
            get => (ICommand)GetValue(ApproveCommandProperty);
            set => SetValue(ApproveCommandProperty, value);
        }

        public ICommand RejectCommand
        {
            get => (ICommand)GetValue(RejectCommandProperty);
            set => SetValue(RejectCommandProperty, value);
        }
    }
}
