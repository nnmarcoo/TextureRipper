   M S B A M L   `   `   `  ����   TextureRipper    TextureRipper.MainWindow   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     �Name$  Window��$	��800��$
��1280�� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=����#23272a � AllowsTransparency$
 True�� � Icon$ Images/icon.ico�� � 	Resources    Font  ProgressBarStyle �� 
TargetType �� Property �� Value	 �� 
TargetType
 ��Name �� CornerRadius ��Name �� Triggers P� Property P� Value �� 
TargetName T� Y��%�  &	        &	 #     '�� �     *�/Fonts/#Louis George Cafe)��� " �.���  � ��
 ���� ��  � ��$ 6����  � ��$ Bottom=���  � ��$ 0q��� " ��.���  � �� �#2c2f33��  � ��
 �������  � �� �#7289da��  � ��$ 1q���  � ��$ 10q���  � ��$ Segoe UI)���  � ��$ 14(���  �@�Cursor$
 Hand����  � �� �� "	 ��.R�� .+��� $
 ButtonBorder��#��z��#��z��#��z��$ 5���� $ ButtonContent��#��z��$��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ ButtonBorder�� � �� �#7289daP�  � ��$
 True��. ��� $ ButtonBorder�� � �� �#7289da�� $ ButtonContent�� � z� T� $ 1Q�.���  � RowDefinitions �� $��40 ��� .+��� -   $
 
CanvasGrid��$��1�����#7289da$��2q����#23272a J� 	AllowDrop$
 True��._�� .+��� $
��1280��$	��800����L�  j� TileMode$
 Tile=� j� Viewport$ 	0,0,32,32� j� ViewportUnits$ Absolute=� L� Drawing �  � GeometryO ��        �@B     �@B @B р� @B р� �� �@B �� �@B @B �    @B 	 � Brush �#25292d�� -    ��Name$ Canvas��
������ $
}�True��$
 True��.+��� -    ��Name$ SourceImage���� -   $ PreviewImage��$��Right=�$��Bottom=��� -   $ 	FileImage��$��images/file.png��$��Center=�$��Center=�$	��100���� -    ��Name$ FileText��$#��Drag or select file to begin.��
������ $��Center=�$��Center=�
�������#��[  $��16(�$��	0,130,0,0q��� -   $ Warning��$�� ��
������ $
��Left=�$	��Top=����#f1c40f#��[  $��16(�$��8,8,0,0q��� -	   $
 Info��$y�Hidden=�
������ $
��Left=�$��Bottom=����#2ecc71#��[  $��20(�$��8,8,0,0q��� $�C : Toggle Keybinds���� �� $�Zoom : Scroll���� �� $�Pan : Left Click Down���� �� $�Modify Point : Left Click���� �� $�Undo Point : CTRL + Z���� -
   ���#2c2f33$��0��._��  � ColumnDefinitions �� $��40 ��� $��40 ��� $
��1120 ��� �� .+��� -   $��0��$
��Left=�$��Center=�$��8,0,0,0q�$��30��$��30��
������ $��0q�.���� $��Images/save.png���� $��1��$��Images/sep.png���� -   $��2��$
��Left=�$��Center=�$��30��$��30��
������ $��0q�.���� $��Images/reset.png���� -   $��3��$��Right=�$��Center=�$��0,0,8,0q�$��30��$��30��
������ $��0q�.���� $��Images/info.png���� -   $��4��$��Right=�$��Center=�$��0,0,8,0q�$��30��$��30��
������ $��0q�.���� $��Images/exit.png��� -    �Name$ ProgressBar��$��1��#��[ ���#2ecc71